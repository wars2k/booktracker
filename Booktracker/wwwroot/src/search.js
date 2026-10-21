let latestSearchRequest = 0;

function searchForBook(event) {
    if (event) {
        event.preventDefault();
    }

    const sessionKey = localStorage.getItem("sessionKey");
    const requestId = ++latestSearchRequest;
    const searchBar = document.getElementById("bookSearchID");
    const resultsValue = document.getElementById("resultsToSearch").value;

    clearProviderError();
    fetch(`/api/books/new?name=${encodeURIComponent(searchBar.value)}&results=${encodeURIComponent(resultsValue)}&sessionKey=${encodeURIComponent(sessionKey ?? "")}`, {
        method: 'GET'
    })
    .then(readApiResponse)
    .then(data => {
        if (requestId === latestSearchRequest) {
            displaySearchResults(data);
        }
    })
    .catch(error => {
        if (requestId !== latestSearchRequest) {
            return;
        }
        document.getElementById("cardContainer").textContent = "";
        searchData = [];
        displayProviderError(error);
    });
}

async function readApiResponse(response) {
    let data = null;

    try {
        data = await response.json();
    } catch (error) {
        if (response.ok) {
            throw createUserError("The server returned an invalid response. Try again later or use Manual Entry.");
        }
    }

    if (!response.ok) {
        const message = [data?.message, data?.details].filter(Boolean).join(" ");
        throw createUserError(message || "Google Books is temporarily unavailable. Try again later or use Manual Entry.");
    }

    return data;
}

function createUserError(message) {
    const error = new Error(message);
    error.userMessage = message;
    return error;
}

function clearProviderError() {
    const existingError = document.getElementById("googleBooksError");
    if (existingError) {
        existingError.remove();
    }
}

function displayProviderError(error) {
    clearProviderError();

    const banner = document.createElement("div");
    banner.id = "googleBooksError";
    banner.classList.add("alert", "alert-danger");
    banner.setAttribute("role", "alert");

    const title = document.createElement("h4");
    title.classList.add("alert-title");
    title.textContent = "Google Books unavailable";

    const message = document.createElement("p");
    message.textContent = error?.userMessage || "Google Books is temporarily unavailable. Try again later or use Manual Entry.";

    banner.append(title, message);
    document.getElementById("row").prepend(banner);
}

//takes the response from the back-end and creates cards for each response.
function displaySearchResults(data) {
    if (!Array.isArray(data)) {
        throw createUserError("The server returned an invalid response. Try again later or use Manual Entry.");
    }

    clearProviderError();
    document.getElementById("cardContainer").textContent = "";
    searchData = data;

    for (let i = 0; i < data.length; i++) {
        createBookResponse(data[i], i);
    }
}

let searchData = [];

//called from the button on the cards in the "add new book page"
//POSTS the Google Books API id of the desired volume.
function addBook(i) {
    const sessionKey = localStorage.getItem("sessionKey");
    const id = searchData[i].id;

    clearProviderError();
    fetch(`/api/books/save?id=${encodeURIComponent(id)}&sessionKey=${encodeURIComponent(sessionKey ?? "")}`, {
        method: 'POST'
    })
    .then(readApiResponse)
    .then(data => createSuccessIndicator(data, i))
    .catch(error => displayProviderError(error));
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
  let button = document.getElementById("button" + index);
  button.classList.add("btn-green");
}