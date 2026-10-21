const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const test = require('node:test');

const workflow = fs.readFileSync(path.join(__dirname, '../.github/workflows/ci.yml'), 'utf8');
const checkout = 'actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7.0.1';
const setupNode = 'actions/setup-node@949feb2413d6458794dcd2491c4babbbce0c15c1 # v7.1.0';
const sdk = 'mcr.microsoft.com/dotnet/sdk:7.0@sha256:a291948a5e5ceb50e3eaa75cb1e6377660146b385e475938d6855e220afaaeed';
const harness = 'mcr.microsoft.com/dotnet/aspnet:7.0@sha256:8a7717ff21c245feacd25dae5ff23306aae0e058578725ad987fd4b8509c36ec';
const actionlintHash = '8aca8db96f1b94770f1b0d72b6dddcb1ebb8123cb3712530b08cc387b349a3d8';

// shortcut: guards this workflow's fixed YAML layout; use a parser if the layout becomes dynamic. actionlint/Actions remain separate gates.
function assertPolicy(source) {
    source = source.replace(/\r\n/g, '\n');
    assert.deepEqual([...source.matchAll(/^([a-z][a-z-]*):/gm)].map(match => match[1]),
        ['name', 'on', 'permissions', 'concurrency', 'defaults', 'jobs']);
    assert.match(source, /^on:\n  pull_request:\n    branches: \[main\]\n  push:\n    branches: \[main\]\n\npermissions:\n  contents: read\n/m);
    assert.equal([...source.matchAll(/^\s*permissions:/gm)].length, 1);
    assert.match(source, /^  group: \$\{\{ github.workflow \}\}-\$\{\{ github.event.pull_request.number \|\| github.ref \}\}$/m);
    assert.match(source, /^  cancel-in-progress: true$/m);
    assert.match(source, /^defaults:\n  run:\n    shell: bash$/m);
    assert.doesNotMatch(source, /pull_request_target|workflow_run|self-hosted|secrets\.|github\.token|GITHUB_TOKEN|continue-on-error:|\|\|\s*true|--if-present|\bdotnet test\b|\bnpm\b|docker (?:build|push|login)\b|upload-artifact|^\s*cache:/m);

    const blocks = source.split(/^jobs:\n/m)[1].split(/(?=^  [a-z][a-z0-9-]*:\n)/m).filter(block => block.trim());
    assert.deepEqual(blocks.map(block => block.split(':')[0].trim()), ['node-checks', 'dotnet-compatibility']);
    for (const block of blocks) {
        assert.match(block, /^    if: github\.repository == 'DaHokeyPokey\/booktracker'$/m);
        assert.match(block, /^    runs-on: ubuntu-24\.04$/m);
        assert.match(block, /^    timeout-minutes: (?:10|20)$/m);
        assert.match(block, /^          persist-credentials: false$/m);
        assert.doesNotMatch(block, /repository\.fork/);
    }
    const [node, dotnet] = blocks;
    assert.match(node, /^    name: Node checks$/m);
    assert.match(dotnet, /^    name: \.NET compatibility$/m);
    assert.deepEqual([...source.matchAll(/^\s+(?:-\s+)?uses: (.+)$/gm)].map(match => match[1]),
        [checkout, setupNode, checkout]);
    assert.match(node, /^          node-version: '24\.22\.1'$/m);
    assert.match(node, /^          package-manager-cache: false$/m);
    assert.match(node, /^        run: node --test Booktracker.Tests\/\*\.test\.js$/m);

    assert.ok(node.includes('https://github.com/rhysd/actionlint/releases/download/v1.7.12/actionlint_1.7.12_linux_amd64.tar.gz'));
    const verify = node.indexOf(actionlintHash + '  $RUNNER_TEMP/actionlint.tar.gz');
    const extract = node.indexOf('tar -xzf');
    const lint = node.indexOf('"$RUNNER_TEMP/actionlint" .github/workflows/*.yml');
    assert.ok(verify >= 0 && extract > verify && lint > extract, 'verify the pinned archive before extraction/execution');
    assert.match(node, /sha256sum --check --strict/);
    assert.match(node, /curl --fail --silent --show-error --location/);

    assert.deepEqual([...dotnet.matchAll(/^\s+(SDK_IMAGE|HARNESS_IMAGE): (.+)$/gm)].map(match => [match[1], match[2]]),
        [['SDK_IMAGE', sdk], ['HARNESS_IMAGE', harness]]);
    assert.match(dotnet, /sh -ec 'dotnet restore Booktracker.Tests\/Booktracker.Tests.csproj\n\s+dotnet build Booktracker.Tests\/Booktracker.Tests.csproj -c Release --no-restore'/);
    assert.deepEqual(dotnet.match(/--network \S+/g), ['--network none']);
    assert.equal((dotnet.match(/docker run --rm --platform linux\/amd64/g) || []).length, 2);
    assert.match(dotnet, /source=\$GITHUB_WORKSPACE\/Booktracker.Tests\/bin\/Release\/net7.0,target=\/checks,readonly/);
    assert.match(dotnet, /--workdir \/tmp/);
    assert.match(dotnet, /"\$HARNESS_IMAGE" dotnet \/checks\/Booktracker.Tests.dll/);
}

test('CI preserves the approved events, privileges, pins and real checks', () => assertPolicy(workflow));
test('CI policy accepts Windows checkout line endings', () => assertPolicy(workflow.replace(/\r?\n/g, '\r\n')));

const unsafeChanges = [
    ['privileged PR trigger', '  pull_request:', '  pull_request_target:'],
    ['write permission', 'contents: read', 'contents: write'],
    ['persisted checkout credentials', 'persist-credentials: false', 'persist-credentials: true'],
    ['self-hosted PR execution', 'runs-on: ubuntu-24.04', 'runs-on: self-hosted'],
    ['wrong repository guard', "github.repository == 'DaHokeyPokey/booktracker'", "github.repository == 'other/booktracker'"],
    ['floating action ref', checkout, 'actions/checkout@v7'],
    ['floating SDK image', sdk, 'mcr.microsoft.com/dotnet/sdk:7.0'],
    ['omitted Node suites', 'node --test Booktracker.Tests/*.test.js', 'echo skipped'],
    ['omitted Release build', 'dotnet build Booktracker.Tests/Booktracker.Tests.csproj -c Release --no-restore', 'echo skipped'],
    ['networked console harness', '--network none', '--network bridge'],
    ['omitted console harness', 'dotnet /checks/Booktracker.Tests.dll', 'echo skipped'],
    ['unreviewed validator checksum', actionlintHash, '0'.repeat(64)],
    ['ignored test failure', 'run: node --test Booktracker.Tests/*.test.js', 'run: node --test Booktracker.Tests/*.test.js || true']
];
for (const [name, before, after] of unsafeChanges) {
    test(`CI policy rejects ${name}`, () => {
        assert.ok(workflow.includes(before), 'mutation must change the real workflow');
        assert.throws(() => assertPolicy(workflow.replace(before, after)));
    });
}
