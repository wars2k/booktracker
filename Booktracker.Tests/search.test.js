const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const test = require('node:test');
const vm = require('node:vm');

const source = fs.readFileSync(path.join(__dirname, '../Booktracker/wwwroot/src/search.js'), 'utf8');
const flush = () => new Promise(resolve => setImmediate(resolve));
const book = title => ({ id: title, title, author: 'Fixture author', imageLink: null });
const response = (status, data) => ({ ok: status >= 200 && status < 300, json: async () => data });
const rateLimit = { message: 'Google Books is temporarily rate limited.', details: 'Try again later or use Manual Entry.' };

// Only DOM operations used by search.js are modeled; card HTML/layout is not under test.
class Element {
    constructor(tagName, id = '') {
        this.tagName = tagName;
        this.id = id;
        this.children = [];
        this.parentNode = null;
        this.value = '';
        this.style = {};
        this.attributes = {};
        this.markupWrites = [];
        this.classes = new Set();
        this.classList = { add: (...names) => names.forEach(name => this.classes.add(name)), contains: name => this.classes.has(name) };
        this.text = '';
    }

    set textContent(value) {
        this.text = String(value);
        this.children.forEach(child => { child.parentNode = null; });
        this.children = [];
    }

    get textContent() {
        return this.text + this.children.map(child => child.textContent).join(' ');
    }

    set innerHTML(value) {
        this.markupWrites.push(String(value));
        this.textContent = value;
    }

    append(...children) {
        children.forEach(child => {
            child.remove();
            child.parentNode = this;
            this.children.push(child);
        });
    }

    prepend(child) {
        child.remove();
        child.parentNode = this;
        this.children.unshift(child);
    }

    remove() {
        if (this.parentNode) {
            this.parentNode.children = this.parentNode.children.filter(child => child !== this);
            this.parentNode = null;
        }
    }

    setAttribute(name, value) { this.attributes[name] = value; }
    addEventListener() {}
}

function fixture() {
    const root = new Element('main');
    const nodes = [];
    const requests = [];
    const allElements = () => {
        const visit = node => [node, ...node.children.flatMap(visit)];
        return visit(root);
    };
    const document = {
        createElement(tag) { const node = new Element(tag); nodes.push(node); return node; },
        getElementById(id) { return allElements().find(node => node.id === id) || null; },
        getElementsByClassName(name) { return allElements().filter(node => node.classList.contains(name)); }
    };
    for (const id of ['row', 'bookSearchID', 'resultsToSearch', 'cardContainer', 'manualEntryButton', 'manualEntryBox',
        'titleManualInput', 'authorManualInput', 'publisherManualInput', 'dateManualInput', 'imageManualInput']) {
        root.append(new Element('div', id));
    }
    document.getElementById('resultsToSearch').value = '12';
    document.getElementById('manualEntryBox').style.display = 'none';
    const context = vm.createContext({
        document,
        localStorage: { getItem: () => 'fixture-session' },
        console: { log() {}, error() {} },
        fetch(url, options) {
            return new Promise((resolve, reject) => requests.push({ url, options, resolve, reject }));
        }
    });
    vm.runInContext(source, context, { filename: 'search.js' });
    context.createBookResponse = (data, index) => {
        const card = document.createElement('div');
        card.classList.add('bookResponse');
        card.textContent = data.title;
        const button = document.createElement('button');
        button.id = `button${index}`;
        card.append(button);
        document.getElementById('cardContainer').append(card);
    };
    return {
        context, document, nodes, requests, allElements,
        search(query) {
            document.getElementById('bookSearchID').value = query;
            context.searchForBook();
            return requests[requests.length - 1];
        },
        banner: () => document.getElementById('googleBooksError'),
        results: () => document.getElementById('cardContainer').textContent,
        data: () => JSON.parse(vm.runInContext('JSON.stringify(searchData)', context))
    };
}

async function populate(f) {
    f.search('saved query').resolve(response(200, [book('Existing result')]));
    await flush();
}

test('failed search clears stale cards and data and shows the provider message', async () => {
    const f = fixture();
    await populate(f);
    f.search('new query').resolve(response(429, rateLimit));
    await flush();
    assert.equal(f.results(), '');
    assert.deepEqual(f.data(), []);
    assert.match(f.banner().textContent, /rate limited/);
    assert.equal(f.banner().attributes.role, 'alert');
});

test('failed save never marks the save button successful', async () => {
    const f = fixture();
    await populate(f);
    f.context.addBook(0);
    f.requests.at(-1).resolve(response(503, { message: 'Google Books is temporarily unavailable.' }));
    await flush();
    assert.equal(f.document.getElementById('button0').classList.contains('btn-green'), false);
    assert.match(f.banner().textContent, /temporarily unavailable/);
});

test('malformed save response cannot mark a book successful', async () => {
    const f = fixture();
    await populate(f);
    f.context.addBook(0);
    f.requests.at(-1).resolve({ ok: true, json: async () => { throw new SyntaxError('private save response'); } });
    await flush();
    assert.equal(f.document.getElementById('button0').classList.contains('btn-green'), false);
    assert.match(f.banner().textContent, /invalid response/);
    assert.doesNotMatch(f.banner().textContent, /private save response/);
});

test('an empty successful search clears previous cards and data', async () => {
    const f = fixture();
    await populate(f);
    f.search('no results').resolve(response(200, []));
    await flush();
    assert.equal(f.results(), '');
    assert.deepEqual(f.data(), []);
    assert.equal(f.banner(), null);
});

test('successful search and save preserve result and success behavior', async () => {
    const f = fixture();
    await populate(f);
    f.context.addBook(0);
    f.requests.at(-1).resolve(response(200, { title: 'Existing result' }));
    await flush();
    assert.equal(f.document.getElementById('button0').classList.contains('btn-green'), true);
    assert.equal(f.banner(), null);
});

test('malformed successful JSON is a visible failure and removes stale results', async () => {
    const f = fixture();
    await populate(f);
    f.search('malformed').resolve({ ok: true, json: async () => { throw new SyntaxError('private response'); } });
    await flush();
    assert.equal(f.results(), '');
    assert.match(f.banner().textContent, /invalid response/);
    assert.doesNotMatch(f.banner().textContent, /private response/);
});

test('successful JSON with the wrong search shape is rejected', async () => {
    const f = fixture();
    f.search('wrong shape').resolve(response(200, { unexpected: true }));
    await flush();
    assert.deepEqual(f.data(), []);
    assert.match(f.banner().textContent, /invalid response/);
});

test('non-JSON failures use safe fallback text', async () => {
    const f = fixture();
    f.search('failure').resolve({ ok: false, json: async () => { throw new SyntaxError('private response'); } });
    await flush();
    assert.match(f.banner().textContent, /temporarily unavailable/);
    assert.doesNotMatch(f.banner().textContent, /private response/);
});

test('network failure uses safe fallback text', async () => {
    const f = fixture();
    f.search('network failure').reject(new Error('private transport details'));
    await flush();
    assert.match(f.banner().textContent, /Manual Entry/);
    assert.doesNotMatch(f.banner().textContent, /private transport details/);
});

test('server error text is not passed to innerHTML and repeated errors replace the banner', async () => {
    const f = fixture();
    const text = '<img src=x onerror="throw new Error()">';
    for (let i = 0; i < 2; i++) {
        f.search('failure').resolve(response(429, { message: text, details: 'Use Manual Entry.' }));
        await flush();
    }
    assert.match(f.banner().textContent, /<img src=x/);
    assert.equal(f.nodes.reduce((count, node) => count + node.markupWrites.length, 0), 0);
    assert.equal(f.allElements().filter(node => node.id === 'googleBooksError').length, 1);
});

test('Manual Entry remains available and posts independently after provider failure', async () => {
    const f = fixture();
    f.search('failure').resolve(response(429, rateLimit));
    await flush();
    assert.ok(f.document.getElementById('manualEntryButton'));
    f.context.manualEntry();
    assert.equal(f.document.getElementById('manualEntryBox').style.display, 'block');
    f.document.getElementById('titleManualInput').value = 'Offline book';
    f.context.gatherManualEntryData();
    const request = f.requests.at(-1);
    assert.equal(new URL(request.url, 'http://fixture.invalid').pathname, '/api/books/new/manual');
    assert.equal(request.options.method, 'POST');
    assert.equal(JSON.parse(request.options.body).title, 'Offline book');
    request.resolve(response(200, {}));
    await flush();
    assert.equal(f.requests.length, 2);
});

test('an older failed search cannot erase a newer successful search', async () => {
    const f = fixture();
    const older = f.search('older');
    const newer = f.search('newer');
    newer.resolve(response(200, [book('Newest result')]));
    await flush();
    older.resolve(response(429, rateLimit));
    await flush();
    assert.match(f.results(), /Newest result/);
    assert.equal(f.data()[0].title, 'Newest result');
    assert.equal(f.banner(), null);
});

test('an older successful search cannot overwrite a newer successful search', async () => {
    const f = fixture();
    const older = f.search('older');
    const newer = f.search('newer');
    newer.resolve(response(200, [book('Newest result')]));
    await flush();
    older.resolve(response(200, [book('Outdated result')]));
    await flush();
    assert.match(f.results(), /Newest result/);
    assert.equal(f.data()[0].title, 'Newest result');
});

test('an older success cannot dismiss the newest search error', async () => {
    const f = fixture();
    const older = f.search('older');
    const newer = f.search('newer');
    newer.resolve(response(429, rateLimit));
    await flush();
    older.resolve(response(200, [book('Outdated result')]));
    await flush();
    assert.equal(f.results(), '');
    assert.deepEqual(f.data(), []);
    assert.match(f.banner().textContent, /rate limited/);
});
