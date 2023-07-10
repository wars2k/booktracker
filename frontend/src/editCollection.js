
let collectionID = null;
checkIfEditIsAllowed()

function checkIfEditIsAllowed() {
    setCollectionID();
    if (collectionID == null) {
        console.log("hello");
        informUnauthorized()
        return;
    }
    fillInCurrentData();
}

function setCollectionID() {
    let queryString = window.location.search;
    let parameters = new URLSearchParams(queryString);
    collectionID = parameters.get('collectionID');
}

function informUnauthorized() {
    document.getElementById("newCollectionForm").innerHTML = "<code>ERROR: No URL parameter 'collectionID' provided.</code>"
}

async function handleEditCollection() {
    if (document.getElementById("collectionName").value == "") {
        document.getElementById("collectionName").classList.add("is-invalid");
        return;
    }
    let editData = gatherEditData();
    let statusCode = await submitEdits(editData);
    if (statusCode == 200) {
        createSuccessBanner();
    }
    console.log(editData);
}

function fillInCurrentData() {
    sessionKey = localStorage.getItem("sessionKey");
    fetch(`/api/collections/${collectionID}?sessionKey=${sessionKey}`, {
    method: 'GET'
    })
    .then(response => response.json())
    .then(data => displayCollectionData(data))
    .catch(error => informError(error));
}

function displayCollectionData(data) {
    console.log(data);
    document.getElementById("collectionName").value = data.name;
    document.getElementById("collectionDescription").value = data.description;
    document.getElementById("collectionImage").value = data.coverImage;
}

function gatherEditData() {
    let editData = {};
    let name = document.getElementById("collectionName").value
    if (name == "") {
        editData.name = null;
    } else {
        editData.name = name;
    }
    let description = document.getElementById("collectionDescription").value
    if (description == "") {
        editData.description = null;
    } else {
        editData.description = description;
    }
    let coverImage = document.getElementById("collectionImage").value;
    if (coverImage == "") {
        editData.coverImage = null;
    } else {
        editData.coverImage = coverImage;
    }
    return editData;
}

async function submitEdits(data) {
    try {
        const sessionKey = localStorage.getItem("sessionKey");
        const response = await fetch(`/api/collections/${collectionID}?sessionKey=${sessionKey}`, {
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
    title.innerHTML = "Unauthorized!"
    let info = document.createElement("p");
    info.classList.add("text-muted");
    info.innerHTML = "Either the desired collection does not exist or is not owned by the logged-in user."
    banner.append(title);
    banner.append(info);
    document.getElementById("page").prepend(banner);
}