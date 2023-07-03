function searchForBook(event) {
    //event.preventDefault();
    sessionKey = localStorage.getItem("sessionKey");
    let searchBar = document.getElementById("bookSearchID");
    fetch(`/api/books/new?name=${searchBar.value}&sessionKey=${sessionKey}`, {
    method: 'GET'
    })
    .then(response => response.json())
    .then(data => displaySearchResults(data))
    .catch(error => console.error(error));
  }
  //takes the response from the back-end and creates cards for each of the 5 responses. 
  function displaySearchResults(data) {
    console.log(data);
    let cards = document.getElementsByClassName("bookResponse");
    let cardsArray = Array.from(cards);
    for (let i = 0; i < cardsArray.length; i++) {
      cardsArray[i].style.display = "none"; 
    }
    for (let i = 0; i < data.length; i++) {
        createBookResponse(data[i], i);
        // let card = document.getElementById("searchCard" + i);
        // let image = document.getElementById("searchCardImage" + i);
        // let title = document.getElementById("searchCardTitle" + i);
        // let author = document.getElementById("searchCardText" + i);
        // let id = document.createElement("div");
        // id.style.display = "none";
        // id.innerText = data[i].id;
        // card.append(id);
        searchData = data;
        // title.innerText = data[i].title;
        // author.innerHTML = data[i].author + "<br>" + data[i].publishedDate
        // image.style.backgroundSize = "contain"
        // image.style.backgroundImage = "url('" + data[i].imageLink + "')";
        // card.style.display="block";
        
        
    }
  }

  let searchData;
  //called from the button on cards in the "add new book page"
  //POSTS the Google Books API id of the desired volume.
  function addBook(i) {
    sessionKey = localStorage.getItem("sessionKey");
    let id = searchData[i].id;
    fetch(`/api/books/save?id=${id}&sessionKey=${sessionKey}`, {
    method: 'POST'
    })
    .then(response => response.json())
    .then(data => console.log(data))
    .catch(error => console.error(error));
    
  }

  function manualEntry() {
    let entryBox = document.getElementById("manualEntryBox");
    entryBox.style.display = "block";
  }
  function gatherManualEntryData() {
    let entryData = {};
    let titleData = document.getElementById("titleManualInput");
    let authorData = document.getElementById("authorManualInput");
    let publisherData = document.getElementById("publisherManualInput");
    let dateData = document.getElementById("dateManualInput");
    let imageData = document.getElementById("imageManualInput")
    entryData.title = titleData.value;
    entryData.author = authorData.value;
    entryData.publisher = publisherData.value;
    entryData.date = dateData.value;
    entryData.image = imageData.value;
    submitManualEntry(entryData);

  }

  function submitManualEntry(entryData) {
    sessionKey = localStorage.getItem("sessionKey");
    fetch(`/api/books/new/manual?sessionKey=${sessionKey}`, {
      method: 'POST',
      body: JSON.stringify(entryData),
      headers: {
        'Content-Type': 'application/json'
      }
      })
      .then(response => response.json())
      .then(data => console.log(data))
      .catch(error => console.error(error));
  }
  function hideManualEntryBox() {
    let entryBox = document.getElementById("manualEntryBox");
    entryBox.style.display = "none";
  }

  function createBookResponse(data, index) {
    let card = document.createElement("div");
    card.classList.add("col-2");
    card.classList.add("bookResponse");
    card.innerHTML = `<div class="card"> \
                        <div class="card-body bookContainer"> \
                          <div class="imageContainer"> \
                            <img src=${data.imageLink}> \
                          </div> \
                          <div class="bookInfo"> \
                            <h3>${data.title}</h3> \
                            <p>${data.author}</p>\
                            <button class="btn" onclick="addBook(${index})">Save</button>\
                          </div>\
                        </div>\
                      </div>`
    document.getElementById("row").append(card);
  }