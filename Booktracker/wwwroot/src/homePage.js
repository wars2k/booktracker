getBooklist();
getCollections();

function getBooklist() {
    let payload = {
        "sessionKey": localStorage.getItem("sessionKey")
    }
    fetch('/api/getBookList', {
        method: 'PUT',
        body: JSON.stringify(payload),
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (response.status === 401) {
            return;
        }
        return response.json()
    })
    .then(data => bookListHandler(data))
    .catch(error => console.error(error));
  }

  function bookListHandler(data) {
    let currentlyReading = parseCurrentlyReading(data);
    let upNext = parseUpNext(data);
    let booksReadThisMonth = parseThisMonth(data);
    displayThisMonth(booksReadThisMonth);
    displayCurrentlyReading(currentlyReading);
    displayUpNext(upNext);
  }

  function parseCurrentlyReading(data) {
    let currentlyReading = [];
    for (let i = 0; i < data.length; i++) {
        let book = data[i];
        if (book.status == "READING") {
            
            currentlyReading.push(book);
        }
        
    }
    return currentlyReading;
  }

  function parseUpNext(data) {
    let upNext = [];
    for (let i = 0; i < data.length; i++) {
        let book = data[i];
        if (book.status == "UP NEXT") {
            
            upNext.push(book);
        }
        
    }
    return upNext;
  }

  function parseThisMonth(data) {
    let thisMonth = [];
    let today = new Date();
    let todayMonth = today.getUTCMonth();
    let todayYear = today.getUTCFullYear();

    for (let i = 0; i < data.length; i++) {
        const book = data[i];
        if (book.dateFinished == "") {
            continue;
        }
        let dateFinished = new Date(book.dateFinished);
        let dateFinishedMonth = dateFinished.getUTCMonth();
        let dateFinishedYear = dateFinished.getUTCFullYear();

        if (todayYear == dateFinishedYear && todayMonth == dateFinishedMonth) {
            thisMonth.push(book);
        }
    }

    return thisMonth;
  }

  function displayCurrentlyReading(books) {
    let currentlyReadingCard = document.getElementById("currentlyReading");
    if (books.length == 0) {
        currentlyReadingCard.innerHTML = "<code>No 'Currently Reading' books found.</code>"
        return;
    }
    for (let i = 0; i < books.length; i++) {
        let image = document.createElement("img");
        let link = document.createElement("a");
        link.href = `bookPage.html?bookListId=${books[i].id}`
        image.src = books[i].imageLink;
        link.append(image);
        image.classList.add("cardImage");
        currentlyReadingCard.append(link);
    }
  }

  function displayUpNext(books) {
    let upNextCard = document.getElementById("upNext");
    if (books.length == 0) {
        upNextCard.innerHTML = "<code>No 'Up Next' books found.</code>"
        return;
    }
    for (let i = 0; i < books.length; i++) {
        let image = document.createElement("img");
        let link = document.createElement("a");
        link.href = `bookPage.html?bookListId=${books[i].id}`
        image.src = books[i].imageLink;
        link.append(image);
        image.classList.add("cardImage");
        upNextCard.append(link);
    }
  }

  function displayThisMonth(booksFinishedThisMonth) {
    let upNextCard = document.getElementById("thisMonth");
    if (booksFinishedThisMonth.length == 0) {
        upNextCard.innerHTML = "<code>No books finished this month.</code>"
        return;
    }
    for (let i = 0; i < booksFinishedThisMonth.length; i++) {
        let image = document.createElement("img");
        let link = document.createElement("a");
        link.href = `bookPage.html?bookListId=${booksFinishedThisMonth[i].id}`
        image.src = booksFinishedThisMonth[i].imageLink;
        link.append(image);
        image.classList.add("cardImage");
        upNextCard.append(link);
    }
  }


  function getCollections() {
    let sessionKey = localStorage.getItem("sessionKey");
    fetch(`/api/collections?include=all&sessionKey=${sessionKey}`, {
        method: 'GET',
    })
    .then(response => {
        if (response.status === 401) {
            return;
        }
        return response.json()
    })
    .then(data => collectionHandler(data))
    .catch(error => console.error(error));
  }

  function collectionHandler(data) {
    displayCollectionTable(data)
  }

  function displayCollectionTable(data) {
    if (data.length == 0) {
        let collectionCard = document.getElementById("collections");
        collectionCard.innerHTML = "<code>No collections found</code>"
    }
    let tableBody = document.getElementById("collectionTableBody");
    for (let i = 0; i < data.length; i++) {
        let collection = data[i];
        let row = document.createElement("tr");
        let title = document.createElement("td");
        title.innerHTML = `<a href=collectionPage.html?collectionID=${collection.collectionID}>${collection.name}</a>`;
        let count = document.createElement("td");
        count.innerText = collection.listOfBookID.length;
        row.append(title);
        row.append(count);
        tableBody.append(row);
    }
  }