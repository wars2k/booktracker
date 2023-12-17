function searchForBook(event) {
    //event.preventDefault();
    sessionKey = localStorage.getItem("sessionKey");
    let searchBar = document.getElementById("bookSearchID");
    let results = document.getElementById("resultsToSearch")
    resultsValue = results.value;
    console.log(resultsValue);
    fetch(`/api/books/new?name=${searchBar.value}&results=${resultsValue}&sessionKey=${sessionKey}`, {
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
    document.getElementById("cardContainer").innerHTML = "";
    for (let i = 0; i < data.length; i++) {
        createBookResponse(data[i], i);
        searchData = data;    
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
    .then(data => createSuccessIndicator(data, i))
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
      .then(data => createSuccessBanner(data))
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
                          <div class="imageContainer" id="imageContainer${index}"> \
                            <img src=${data.imageLink} id="image${index}"> \
                          </div> \
                          <div class="bookInfo"> \
                            <h3>${data.title}</h3> \
                            <p>${data.author}</p>\
                            <button class="btn" id="button${index}" onclick="addBook(${index})">Save</button>\
                          </div>\
                        </div>\
                      </div>`
    document.getElementById("cardContainer").append(card);
    if (data.imageLink == null) {  
      document.getElementById(`image${index}`).src = "styles/placeholder-image.png";
    }
  }

  function watchForEnter() {
    let input = document.getElementById("bookSearchID");
    input.addEventListener("keydown", function(event) {
      if (event.key === "Enter") {
        searchForBook();
      }
    })
  }

  watchForEnter();

  function createSuccessBanner(data) {
    let banner = document.createElement("div");
    banner.classList.add("alert");
    banner.classList.add("alert-success");
    let title = document.createElement("h4");
    title.classList.add("alert-title");
    title.innerHTML = "Success!"
    banner.append(title);
    document.getElementById("row").prepend(banner);
}

function createSuccessIndicator(data, index) {
  console.log(data);
  let button = document.getElementById("button" + index);
  button.classList.add("btn-green");
}