function getCollections() {
    sessionKey = localStorage.getItem("sessionKey");
    fetch(`http://localhost:5000/api/collections?include=all&sessionKey=${sessionKey}`, {
    method: 'GET'
    })
    .then(response => response.json())
    .then(data => displayCollectionData(data))
    .catch(error => console.error(error));
}

async function displayCollectionData(data) {
    if (data.length == 0) {
        displayNoCollectionsAlert()
        return;
    }
    console.log(data);
    let container = document.getElementById("container");
    for (let i = 0; i < data.length; i++) {
        let collection = data[i];
        let row = document.createElement("div");
        row.classList.add("col-sm-12");
        row.style.width = "100%";
        row.style.marginBottom = "10px";
        let card = document.createElement("div");
        card.classList.add("card");
        card.style.overflowY = "auto";
        let header = document.createElement("div");
        header.classList.add("card-header");
        card.append(header);
        let title = document.createElement("h3");
        title.classList.add("card-title");
        header.append(title);
        title.innerHTML = `<a href="collectionPage.html?collectionID=${collection.collectionID}">${collection.name}</a>`;
        let body = document.createElement("div");
        body.classList.add("card-body");
        body.id = "collection" + collection.collectionID;
        
        card.append(body);
        row.append(card);
        if (data[i].listOfBookID.length > 0) {
            let imageURLs = await getImagesFromBookListID(data[i].listOfBookID);
            for (let j = 0; j < imageURLs.length; j++) {
                
                let cardBody = document.getElementById(`collection${collection.collectionID}`);
                
                body.innerHTML += `<a href="bookPage.html?bookListId=${data[i].listOfBookID[j]}"><img src="${imageURLs[j]}" class="coverCard"></a>`;
            }
        } else {
            body.innerHTML = "This collection doesn't have any books yet.";
        }
        container.append(row);
        
    }
}

async function getImagesFromBookListID(listOfBookID) {
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

getCollections();

function displayNoCollectionsAlert() {
    let row = document.createElement("div");
        row.classList.add("col-sm-12");
        row.style.width = "100%";
        row.style.marginBottom = "10px";
        let card = document.createElement("div");
        card.classList.add("card");
        card.style.overflowY = "auto";
        let header = document.createElement("div");
        header.classList.add("card-header");
        card.append(header);
        let title = document.createElement("h3");
        title.classList.add("card-title");
        header.append(title);
        title.innerHTML = `Error: No collections found.`;
        let body = document.createElement("div");
        body.classList.add("card-body");
        body.innerHTML = "<code>Create a new collection now!</code>"
        card.append(body);
        row.append(card);
        let container = document.getElementById("container");
        container.append(row);
}