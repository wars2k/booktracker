getBooklist();
getCollections();

function getBooklist() {
    let payload = {
        "sessionKey": localStorage.getItem("sessionKey")
    }
    fetch('http://localhost:5000/api/getBookList', {
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


  function getCollections() {
    let sessionKey = localStorage.getItem("sessionKey");
    fetch(`http://localhost:5000/api/collections?include=all&sessionKey=${sessionKey}`, {
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