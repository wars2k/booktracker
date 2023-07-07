getBooklist();

function getBooklist() {
    let payload = {
        "sessionKey": localStorage.getItem("sessionKey")
    }
    fetch('http://localhost:5000/api/getBookList', {
        method: 'PUT',
        body: JSON.stringify(payload),
        header: {
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
            console.log(book.status);
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
            console.log(book.status);
            upNext.push(book);
        }
        
    }
    return upNext;
  }

  function displayCurrentlyReading(books) {
    let currentlyReadingCard = document.getElementById("currentlyReading");
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