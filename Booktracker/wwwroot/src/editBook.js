function updateStatusView(status, classes) {
    event.preventDefault();
    let editStatusBox = document.getElementById("status");
    editStatusBox.className = "";
    editStatusBox.classList.add("status");
    editStatusBox.classList.add(classes);
    editStatusBox.innerText = status;
}

function updateRatingView(content, classes) {
    event.preventDefault();
    let editRatingBox = document.getElementById("rating");
    editRatingBox.className = "";
    editRatingBox.classList.add("status");
    editRatingBox.classList.add(classes);
    editRatingBox.innerText = content.innerText;

}

let bookListID = null;
let globalData = null;
checkIfEditIsAllowed()

function checkIfEditIsAllowed() {
    setBookListID();
    if (bookListID == null) {
        console.log("hello");
        informUnauthorized()
        return;
    }
    fillInCurrentData();
}

function setBookListID() {
    let queryString = window.location.search;
    let parameters = new URLSearchParams(queryString);
    bookListID = parameters.get('bookListID');
}

function informUnauthorized() {
    document.getElementById("newCollectionForm").innerHTML = "<code>ERROR: No URL parameter 'bookListID' provided.</code>"
}

function fillInCurrentData() {
    sessionKey = localStorage.getItem("sessionKey");
    fetch(`/api/Booklist/${bookListID}/data?sessionKey=${sessionKey}`, {
    method: 'GET',
    })
    .then(response => response.json())
    .then(data => displayBookData(data))
    .catch(error => informError(error));
}

function displayBookData(data) {
    globalData = data;
    console.log(data);
    document.getElementById("title").value = data.title;
    document.getElementById("author").value = data.author;
    document.getElementById("publishedDate").value = data.publisher;
    document.getElementById("publisher").value = data.publishedDate;
    document.getElementById("imageLink").value = data.imageLink;
    document.getElementById("description").value = data.description;
    document.getElementById("pageCount").value = data.pageCount;
    document.getElementById("isbn").value = data.isbn;
    document.getElementById("category").value = data.category;
}

async function handleEdit() {
    if (document.getElementById("title").value == "") {
        document.getElementById("title").classList.add("is-invalid");
        return;
    }
    let editData = gatherEditData();
    let statusCode = await submitEdits(editData);
    if (statusCode == 200) {
        createSuccessBanner();
    }
    console.log(editData);
}

function gatherEditData() {
    let editData = {};
    editData.bookID =  globalData.bookID;
    editData.title = document.getElementById("title").value;
    editData.author = document.getElementById("author").value;
    editData.datePublished = document.getElementById("publishedDate").value;
    editData.publisher = document.getElementById("publisher").value;
    editData.imageLink = document.getElementById("imageLink").value;
    editData.description = document.getElementById("description").value;
    editData.pageCount = document.getElementById("pageCount").value;
    editData.isbn = document.getElementById("isbn").value;
    editData.category = document.getElementById("category").value;
    return editData;
}

async function submitEdits(data) {
    try {
        const sessionKey = localStorage.getItem("sessionKey");
        const response = await fetch(`/api/Booklist/${bookListID}/data?sessionKey=${sessionKey}`, {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify(data)
        });
        const statusCode = response.status;
        return statusCode;
      } catch (error) {
        console.error(error);
        throw error;
      }
}

function createSuccessBanner() {
    let banner = document.createElement("div");
    banner.classList.add("alert");
    banner.classList.add("alert-success");
    let title = document.createElement("h4");
    title.classList.add("alert-title");
    title.innerHTML = "Success!"
    banner.append(title);
    document.getElementById("page").prepend(banner);
}

function informError() {
    let banner = document.createElement("div");
    banner.classList.add("alert");
    banner.classList.add("alert-danger");
    let title = document.createElement("h4");
    title.classList.add("alert-title");
    title.innerHTML = "ERROR"
    let info = document.createElement("p");
    info.classList.add("text-muted");
    info.innerHTML = "Unknown error."
    banner.append(title);
    banner.append(info);
    document.getElementById("page").prepend(banner);
}