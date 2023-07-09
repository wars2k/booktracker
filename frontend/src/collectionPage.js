let globalCollectionID = null;
let globalImageIDs = null;
let isEditModeActive = false;
let booksToBeRemoved = [];

initiateCollectionDisplay();

document.getElementById("editCollectionButton").href = `editCollection.html?collectionID=${globalCollectionID}`;

//gets the collectionID from the url. If there isn't one, it puts an error on the screen. If there is, we call a function to get the collection Data
function initiateCollectionDisplay() {
    let collectionID = getCollectionIDfromURL();
    if (collectionID == null) {
        document.getElementById("collectionDataContainer").innerHTML = "<code>No 'collectionID' provided in URL.</code>";
        document.getElementById("addBookCard").style.display = "none";
        return;
    }
    getCollectionData(collectionID);
}

function getCollectionIDfromURL() {
    let queryString = window.location.search;
    let parameters = new URLSearchParams(queryString);
    let collectionID = parameters.get('collectionID');
    globalCollectionID = collectionID;
    return collectionID;
}

//gets collection data and passes it into a data handler
function getCollectionData(collectionID) {
    sessionKey = localStorage.getItem("sessionKey");
    fetch(`http://localhost:5000/api/collections/${collectionID}?sessionKey=${sessionKey}`, {
    method: 'GET'
    })
    .then(response => response.json())
    .then(data => handleCollectionData(data))
    .catch(error => handleCollectionError(error));
}

function handleCollectionError(error) {
    let collectionCard = document.getElementById("cardHeader");
    document.getElementById("collectionDataContainer").style.display = "none";
    collectionCard.innerHTML = "<code>ERROR: Unable to get collection data. Either requested collection does not exist or current user is not authorized to access it.</code>";
    document.getElementById("addBookCard").style.display = "none";
}

//calls a function that gets each book's image link from the list of book IDs. Then calls functions to display that data.
async function handleCollectionData(data) {
    let imageLinks = await getBookDataFromBookListArray(data.listOfBookID);
    displayCollectionData(data);
    displayImageLinks(imageLinks, data.listOfBookID);
    getBookListData(data.listOfBookID);
}

async function getBookDataFromBookListArray(listOfBookID) {
    let imageLinks = [];
    for (let i = 0; i < listOfBookID.length; i++) {
        const id = listOfBookID[i];
        try {
            const sessionKey = localStorage.getItem("sessionKey");
            const body = {
                "sessionKey": sessionKey
            }
            const response = await fetch(`http://localhost:5000/api/Booklist/${id}/data`, {
              method: 'PUT',
              headers: {
                'Content-Type': 'application/json'
              },
              body: JSON.stringify(body)
            });
            const data = await response.json();
            imageLinks.push(data.imageLink);
          } catch (error) {
            console.error(error);
            throw error;
          } 
    }
    
    return imageLinks;
}

function displayCollectionData(data) {
    document.getElementById("collectionTitle").innerText = data.name;
    document.getElementById("createdDate").innerText += " " + data.createdDate.split(' ')[0];
    document.getElementById("description").innerHTML = data.description;
}

function displayImageLinks(imageLinks, imageIDs) {
    console.log(imageIDs);
    globalImageIDs = imageIDs;
    let body = document.getElementById("collectionBookContainer");
    for (let j = 0; j < imageLinks.length; j++) {
        //body.innerHTML += `<a href="bookPage.html?bookListId=${imageIDs[j]}"><img src="${imageLinks[j]}" class="coverCard"></a>`;
        body.innerHTML += `<a onclick="handleClick(${imageIDs[j]})" style="cursor: pointer" id="coverCard${imageIDs[j]}"><img src="${imageLinks[j]}" class="coverCard"></a>`;
    }
}

function getBookListData(alreadyAddedIDs) {
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
            informIncorrectPassword()
        }
        return response.json()
    })
    .then(data => createBookListTable(data, alreadyAddedIDs))
    .catch(error => console.error(error));
}

function createBookListTable(data, alreadyAddedIDs) {
    let tableBody = document.getElementById("bookListTableBody");
    for (let i = 0; i < data.length; i++) {
        const bookData = data[i];
        if (alreadyAddedIDs.includes(bookData.id)) {
            continue;
        }
        let row = document.createElement("tr");
        let title = document.createElement("td");
        let author = document.createElement("td");
        let addButton = document.createElement("td");
        title.innerText = bookData.title;
        author.innerText = bookData.author;
        addButton.innerHTML = `<button class='btn tableButton' onclick='addToCollection(${bookData.id})'>Add</button`
        row.append(title);
        row.append(author);
        row.append(addButton);
        tableBody.append(row);
    }
}
function handleClick(bookListID) {
    if (isEditModeActive) {
        highlightBook(bookListID)
    } else {
        window.location.href = `bookPage.html?bookListId=${bookListID}`;
    }
}
function addToCollection(bookListID) {
    sessionKey = localStorage.getItem("sessionKey");
    fetch(`http://localhost:5000/api/collections/${globalCollectionID}/add/${bookListID}?sessionKey=${sessionKey}`, {
    method: 'POST'
    })
    .then(response => response.json())
    .then(data => refreshPage(data))
    .catch(error => refreshPage(error));
}

function refreshPage(data) {
    location.reload();
}

function highlightBook(bookListID) {
    let coverCard = document.getElementById(`coverCard${bookListID}`);
    if (booksToBeRemoved.includes(bookListID)) {
        coverCard.style.border = "";
        let index = booksToBeRemoved.indexOf(bookListID);
        booksToBeRemoved.splice(index, 1);
        return;
    }
    coverCard.style.border = "2px solid red";
    booksToBeRemoved.push(bookListID);

}

function initiateEditMode() {
    isEditModeActive = true;
    displayEditModeBanner();
}

function displayEditModeBanner() {
    let banner = document.createElement("div");
    banner.id = "banner";
    banner.classList.add("alert");
    banner.classList.add("alert-info");
    let title = document.createElement("h4");
    title.classList.add("alert-title");
    title.innerHTML = "Remove Books from Collection"
    let info = document.createElement("p");
    info.classList.add("text-muted");
    info.innerHTML = "Click to select the books that should be removed."
    let buttons = document.createElement("div");
    buttons.classList.add("btn-list");
    buttons.innerHTML = "<button class=btn onclick='submitDeletes()'>Delete Selected Books</button>"
    buttons.innerHTML += "<button class=btn onclick='cancelEditMode()'>Cancel</button>"
    banner.append(title);
    banner.append(info);
    banner.append(buttons);
    document.getElementById("container").prepend(banner);
}

function cancelEditMode() {
    if (booksToBeRemoved.length > 0) {
        for (let i = 0; i < booksToBeRemoved.length; i++) {
            let coverCard = document.getElementById(`coverCard${booksToBeRemoved[i]}`);
            coverCard.style.border = ""
        }
        isEditModeActive = false;
    }
    document.getElementById("banner").style.display = "none";
    booksToBeRemoved = [];
}

function submitDeletes() {
    if (booksToBeRemoved.length == 0) {
        return;
    }
    for (let i = 0; i < booksToBeRemoved.length; i++) {
        sessionKey = localStorage.getItem("sessionKey");
        fetch(`http://localhost:5000/api/collections/${globalCollectionID}/remove/${booksToBeRemoved[i]}?sessionKey=${sessionKey}`, {
        method: 'DELETE'
        })
        .then(response => response.json())
        .then(data => console.log(data))
        .catch(error => console.log(data));
    }
    refreshPage();
}

function toggleModal() {
    if (document.getElementById("deleteModal").style.display == "none") {
        document.getElementById("deleteModal").style.display = "block";
    } else {
        document.getElementById("deleteModal").style.display = "none"
    }
}

function submitCollectionDelete() {
    sessionKey = localStorage.getItem("sessionKey");
    fetch(`http://localhost:5000/api/collections/${globalCollectionID}?sessionKey=${sessionKey}`, {
    method: 'DELETE'
    })
    .then(response => response.json())
    .then(data => refreshPage(data))
    .catch(error => console.log(error));
}