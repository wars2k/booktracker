fillCollections();

/**
 * Entry point for collection workflow. Gets all collections for a 
 * given book, gets current collections for a given book, then 
 * builds a dropdown with the info.
 */
async function fillCollections() {
    let currentCollections = await getCurrentCollections();

    let allCollections = await getAllCollections();

    buildDropdown(currentCollections, allCollections);
}

/**
 * Gets an array of collection IDs from the server. 
 * @returns An array of collection IDs that the current book is already added to.
 */
async function getCurrentCollections() {
    let sessionKey = localStorage.getItem("sessionKey");
    let bookListID = getBookIDfromURL();

    return fetch(`/api/collections?include=name&bookListID=${bookListID}&sessionKey=${sessionKey}`, {
          method: 'GET',
    })
    .then(response => {
        if (response.status === 401) {
            informIncorrectPassword()
        }
          
        return response.json()
    })
        .then(data => {
        
        return data;
    })
}

/**
 * Gets info for all collections. 
 * @returns An array of collection objects containing title and ID.
 */
async function getAllCollections() {
    let sessionKey = localStorage.getItem("sessionKey");

    return fetch(`/api/collections?include=name&sessionKey=${sessionKey}`, {
          method: 'GET',
    })
    .then(response => {
        if (response.status === 401) {
            informIncorrectPassword()
        }
          
        return response.json()
    })
        .then(data => {
        
        return data;
    })
}

/**
 * For each collection the user has created, adds an option to a drop-down list. 
 * If the current book is already added to a collection, it is checked off. 
 * Also adds an event listener to handle adding/removing books from collections.
 * @param {Array} currentCollections 
 * @param {Array} allCollections 
 */
function buildDropdown(currentCollections, allCollections) {
    
    let container = document.getElementById("collectionContainer");

    for (let i = 0; i < allCollections.length; i++) {
        let collection = allCollections[i];
        
        let wrapper = document.createElement("div");
        wrapper.classList.add("dropdown-item");
        wrapper.addEventListener("click", function(e) { handleCollectionClick(collection.collectionID, e, wrapper, currentCollections)})

        let input = document.createElement("input");
        input.classList.add("form-check-input");
        input.type = "checkbox";
        input.value = collection.collectionID;
        
        if (currentCollections.includes(collection.collectionID)) {
            input.checked = true;
        }

        let span = document.createElement("span");
        span.innerText = collection.collectionName;
        span.style.marginLeft = "10px";
        wrapper.append(input);
        wrapper.append(span);
        container.append(wrapper);
    }

}

/**
 * Determines whether or not the user wants to add or remove the book to a given collection. 
 * Once determined, calls a function that does it.
 * @param {String} ID The ID of the collection that was clicked.
 * @param {*} e 
 * @param {HTMLElement} wrapper The wrapper element containing the checkbox and name of the collection.
 * @param {Array} currentCollections An array of all the collections the book is already contained in.
 */
async function handleCollectionClick(ID,e, wrapper, currentCollections) {
    console.log("test");
    e.stopPropagation();

    //determine if it is currently clicked or not clicked
    if (currentCollections.includes(ID)) {
        await removeBookFromCollection(ID);
    } else {
        await addBookToCollection(ID);
    }

    location.reload();
}

/**
 * Removes the book whose page the user is on from the given collection.
 * @param {String} id ID of the collection.
 * @returns Null
 */
async function removeBookFromCollection(id) {
    let sessionKey = localStorage.getItem("sessionKey");
    let bookListID = getBookIDfromURL();

    return fetch(`/api/collections/${id}/remove/${bookListID}?sessionKey=${sessionKey}`, {
          method: 'DELETE',
    })
    .then(response => {
        if (response.status === 401) {
            informIncorrectPassword()
        }
          
        return
    })
        .then(data => {
        
        return data;
    })
}

/**
 * Adds the book whose page the user is on to the given collection.
 * @param {String} id ID of the collection
 * @returns Null
 */
async function addBookToCollection(id) {
    let sessionKey = localStorage.getItem("sessionKey");
    let bookListID = getBookIDfromURL();

    return fetch(`/api/collections/${id}/add/${bookListID}?sessionKey=${sessionKey}`, {
          method: 'POST',
    })
    .then(response => {
        if (response.status === 401) {
            informIncorrectPassword()
        }
          
        return
    })
        .then(data => {
        
        return data;
    })
}