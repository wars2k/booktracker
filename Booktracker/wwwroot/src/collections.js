function getCollections() {
    sessionKey = localStorage.getItem("sessionKey");
    fetch(`/api/collections?include=all&sessionKey=${sessionKey}`, {
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
        body.style.overflowX = "auto";
        body.id = "collection" + collection.collectionID;
        
        card.append(body);
        row.append(card);
        if (data[i].listOfBookID.length > 0) {
            let imageURLsAndTitles = await getImagesFromBookListID(data[i].listOfBookID);
            let maxLength = Math.min(imageURLsAndTitles.titles.length, 10);
            let imageURLs = imageURLsAndTitles.imageLinks;
            let titles = imageURLsAndTitles.titles;
            for (let j = 0; j < maxLength; j++) {
                
                if (imageURLs[j] == "styles/placeholder-image.png") {
                    body.innerHTML += `<a href="bookPage.html?bookListId=${data[i].listOfBookID[j]}" title="${titles[j]}" style="border: 1px solid #e2e8f0"><img src="${imageURLs[j]}" class="coverCard"></a>`;
                } else {
                    body.innerHTML += `<a href="bookPage.html?bookListId=${data[i].listOfBookID[j]}" title="${titles[j]}" ><img src="${imageURLs[j]}" class="coverCard"></a>`;
                }
            }
        } else {
            body.innerHTML = "This collection doesn't have any books yet.";
        }
        container.append(row);
        
    }
}

async function getImagesFromBookListID(listOfBookID) {
    let imageLinks = [];
    let titles = [];
    let maxLength = Math.min(listOfBookID.length, 10);
    for (let i = 0; i < maxLength; i++) {
        const id = listOfBookID[i];
        try {
            const sessionKey = localStorage.getItem("sessionKey");
            const body = {
                "sessionKey": sessionKey
            }
            const response = await fetch(`/api/Booklist/${id}/data?sessionKey=${sessionKey}`, {
              method: 'GET',
              
            });
            const data = await response.json();
            imageLinks.push(data.imageLink);
            titles.push(data.title);
          } catch (error) {
            console.error(error);
            throw error;
          } 
    }
    
    return {
        "imageLinks": imageLinks,
        "titles": titles
    }
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