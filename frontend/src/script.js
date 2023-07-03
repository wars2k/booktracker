//queries the back-end for a certain book title, then calls displaySearchResults() to display the results on screen
function searchForBook() {
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
    for (let i = 0; i < 6; i++) {
        let card = document.getElementById("searchCard" + i);
        let image = document.getElementById("searchCardImage" + i);
        let title = document.getElementById("searchCardTitle" + i);
        let author = document.getElementById("searchCardText" + i);
        let id = document.createElement("div");
        id.style.display = "none";
        id.innerText = data[i].id;
        card.append(id);
        searchData = data;
        title.innerText = data[i].title;
        author.innerHTML = data[i].author + "<br>" + data[i].publishedDate
        image.style.backgroundSize = "contain"
        image.style.backgroundImage = "url('" + data[i].imageLink + "')";
        card.style.display="block";
        
        
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
  
  //creates a new row for each book. Also creates a button used for editing the entry. 
  function createBookTable(bookList) {
    let table = document.getElementById("mainTableBody");
    table.innerHTML = "";
    for (let i = 0; i < bookList.length; i++) {
      let row = document.createElement("tr");
        let id = document.createElement("td");
        id.innerText = bookList[i].id;
        row.append(id);
        let title = document.createElement("td");
        title.innerText = bookList[i].title;
        row.append(title);
        let author = document.createElement("td");
        author.innerText = bookList[i].author;
        row.append(author);
        let publisher = document.createElement("td");
        if (bookList[i].publisher != null) {
          publisher.innerText = bookList[i].publisher;
        }
        row.append(publisher);
        let publishedDate = document.createElement("td");
        if (bookList[i].publishedDate != null) {
          publishedDate.innerText = bookList[i].publishedDate;
        }
        row.append(publishedDate);
        let editButtonData = document.createElement("td");
        let editButton = document.createElement("button");
        editButtonData.append(editButton);
        editButton.innerText = "Edit";
        editButton.classList.add("btn");
        editButton.style.height = "25px";
        editButton.style.width = "50px"
        editButton.addEventListener("click", function() {
          editBookEntry(bookList[i].id);
        })
        row.append(editButtonData);

      table.append(row); 
    }
  }
  //queries the database for a list of all books, then calls createBookTable() to display the queried data.
  //the back-end returns the data in an array of objects with the following properties:
  //   id, title, author, publisher, publishedDate
  function getAllBooks() {
    fetch('/api/books', {
      method: 'GET'
    })
    .then(response => response.json())
    .then(data => createBookTable(data))
    .catch(error => console.error(error));
  }

  getAllBooks();

  //called from the buttons on each row.
  //First, removes all old eventListeners by cloning the button. 
  //Then, Adds a new event listener to the "submit" button that does the following: 
  // 1. gathers the edits entered in the edit box
  // 2. hides the edit box
  // 3. updates the book table
  function editBookEntry(id) {
    let submitButton = document.getElementById("submitEditButton");
    let clone = submitButton.cloneNode(true);
    submitButton.parentNode.replaceChild(clone, submitButton);
    let deleteButton = document.getElementById("deleteBookButton");
    let deleteClone = deleteButton.cloneNode(true);
    deleteButton.parentNode.replaceChild(deleteClone, deleteButton);
    let editBox = document.getElementById("editBox");
    editBox.style.display = "block";
    clone.addEventListener("click", function() {
      gatherEdits(id);
      console.log("test");
      hideEditBox();
      getAllBooks();
      
    })
    deleteClone.addEventListener("click", function() {
      deleteBook(id);
      hideEditBox();
    })

    
  }

  function hideEditBox() {
    let editBox = document.getElementById("editBox");
    editBox.style.display = "none";
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

  //gathers the value from each edit input into one object called "editData", then submits the edits to the backend. 
  function gatherEdits(id) {
    let editData = {};
    let titleEdit = document.getElementById("titleEditInput");
    let authorEdit = document.getElementById("authorEditInput");
    let publisherEdit = document.getElementById("publisherEditInput");
    let dateEdit = document.getElementById("dateEditInput");
    editData.title = titleEdit.value;
    editData.author = authorEdit.value;
    editData.publisher = publisherEdit.value;
    editData.date = dateEdit.value;
    editData.id = id;
    console.log(editData);
    submitEdits(editData);
  }

  //submits edits to the back end. ID goes after the first slash, then the edits are entered as key/value pairs. 
  function submitEdits(editData) {
    sessionKey = localStorage.getItem("sessionKey");
    fetch(`/api/books/${editData.id}?title=${editData.title}&author=${editData.author}&publisher=${editData.publisher}&date=${editData.date}&sessionKey=${sessionKey}`, {
      method: 'PUT'
      })
      .then(response => response.json())
      .then(data => console.log(data))
      .catch(error => console.error(error));
  }

  function deleteBook(id) {
    sessionKey = localStorage.getItem("sessionKey");
    fetch(`/api/books/${id}/delete?sessionKey=${sessionKey}`, {
      method: 'DELETE'
    })
    .then(response => response.json())
    .then(data => console.log(data))
    .catch(error => console.error(error));
  }

  function searchTable() {
    // Get input element and table element
    var input = document.getElementById("databaseSearch");
    var table = document.getElementById("mainTable");
  
    // Get all rows in the table
    var rows = table.getElementsByTagName("tr");
  
    // Loop through all rows and hide those that do not match the search query
    for (var i = 0; i < rows.length; i++) {
      var row = rows[i];
      var cells = row.getElementsByTagName("td");
      var showRow = false;
  
      // Loop through all cells in the row and check for a match with the search query
      for (var j = 0; j < cells.length; j++) {
        var cell = cells[j];
        if (cell.innerHTML.toLowerCase().indexOf(input.value.toLowerCase()) > -1) {
          showRow = true;
          break;
        }
      }
  
      // Show or hide the row based on the search query
      if (showRow) {
        row.style.display = "";
      } else {
        row.style.display = "none";
      }
    }
  }
