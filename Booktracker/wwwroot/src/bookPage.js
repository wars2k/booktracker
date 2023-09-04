let globalBookListID = null;
setUpBookPage()

document.getElementById('editButton').href = `editBook.html?bookListID=${globalBookListID}`

//gets the bookList ID located in the URL that is passed from the bookList table.
function getBookIDfromURL() {
    let urlParams = new URLSearchParams(window.location.search);
    bookListID = urlParams.get('bookListId');
    globalBookListID = bookListID;
    return bookListID
}

async function setUpBookPage() {
    let id = getBookIDfromURL();
    let data = await getBookData(id);
    fillBookData(data);
    fillBookMetaData(data);
    console.log(data);
}

function getBookData(id) {
    let sessionKey = localStorage.getItem("sessionKey");
      
      return fetch(`/api/Booklist/${id}/data?sessionKey=${sessionKey}`, {
          method: 'GET',
      })
      .then(response => {
          if (response.status === 401) {
              informIncorrectPassword()
          }
          console.log("TEST");
          return response.json()
      })
      .then(data => {
        console.log("TEST2")
        return data;
      })
      .catch(error => console.error(error));
}

function fillBookData(data) {

    let title = document.getElementById("title");
    title.innerHTML = "<i>" + data.title + "<i>";

    let author = document.getElementById("author");
    author.innerText = data.author;

    let publisher = document.getElementById("publisher");
    publisher.innerText = data.publishedDate;

    let category = document.getElementById("category");
    category.innerText = data.category;

    let pageCount = document.getElementById("pageCount");
    pageCount.innerText = data.pageCount + " pages";

    let isbn = document.getElementById("isbn");
    isbn.innerText = data.isbn;

    let description = document.getElementById("description");
    description.innerHTML = data.description;

    let coverImage = document.getElementById("coverImage");
    coverImage.src = data.imageLink;

}

function fillBookMetaData(data) {

    let startDate = document.getElementById("startDate");
    startDate.innerText = data.dateStarted;

    let finishDate = document.getElementById("finishDate");
    finishDate.innerText = data.dateFinished;

    let owner = document.getElementById("owner");
    owner.innerText = data.username;

    let bookListID = document.getElementById("bookListID");
    bookListID.innerText = data.id;

    let rating = document.getElementById("rating");
        if (data.rating != null) {
            rating.classList.add("status");
            switch (data.rating) {
              case "1":
                rating.classList.add("status-red");
                rating.innerHTML = "&#9734";
                break;
              case "2":
                rating.classList.add("status-orange");
                rating.innerHTML = "&#9734&#9734"
                break;
              case "3":
                rating.classList.add("status-yellow");
                rating.innerHTML = "&#9734&#9734&#9734";
                break;
              case "4":
                rating.classList.add("status-blue");
                rating.innerHTML = "&#9734&#9734&#9734&#9734";
                break;
              case "5":
                rating.classList.add("status-green");
                rating.innerHTML = "&#9734&#9734&#9734&#9734&#9734";
                break;
            }
        }

        let status = document.getElementById("status");
        if (data.status != null) {
            status.innerText = data.status;
            status.classList.add("status");
            status.classList.add("statusChanges");
            
            switch (data.status) {
              case "UNASSIGNED":
                status.classList.add("status-yellow")
                break;
              case "READING":
                status.classList.add("status-green")
                break;
              case "UP NEXT":
                status.classList.add("status-teal");
                break;
              case "WISHLIST":
                status.classList.add("status-blue");
                break;
              case "FINISHED":
                status.classList.add("status-purple");
                break;
              case "DNF":
                status.classList.add("status-red");
                break;
              case "TO READ":
                status.classList.add("status-azure");
                break;
            }
        }
}
//journal section

let journalData

function showJournalPane() {
  document.getElementById("journalEditorPane").style.display = "flex";
  document.getElementById("saveEntryButton").style.display = "inline";
  document.getElementById("deleteEntryButton").style.display = "inline";
  document.getElementById("addEntryButton").style.display = "inline";
}

function hideJournalPane() {
  currentID = null;
  document.getElementById("journalEditorPane").style.display = "none";
  document.getElementById("saveEntryButton").style.display = "none";
  document.getElementById("deleteEntryButton").style.display = "none";
  document.getElementById("addEntryButton").style.display = "none";
}

async function journalInitHandler() {

  let data;
  let bookListId = getBookIDfromURL();

  showJournalPane()
  tinymce.get("tinymce-default").setContent("Choose an entry to the right to start editing!");
  if (journalData == null || hasBeenRecentSave) {
    data = await getJournalData(bookListId);
    journalData = data;
  } else {
    data = journalData;
  }

  buildJournalTable(data);

  tinymce.get('tinymce-default').getBody().setAttribute('contenteditable', false);
  

}

function getJournalData(id) {
  let sessionKey = localStorage.getItem("sessionKey");
      
      return fetch(`/api/journal/${id}/entries?sessionKey=${sessionKey}`, {
          method: 'GET',
      })
      .then(response => {
          if (response.status === 401) {
          
          }
          return response.json()
      })
      .then(data => {
        
        return data;
      })
      .catch(error => console.error(error));
}

function buildJournalTable(data) {
  let tableBody = document.getElementById("journalEntryTableBody")
  tableBody.innerHTML = "";
  for (let i = 0; i < data.length; i++) {
    const entry = data[i];
    let row = document.createElement("tr");
    row.classList.add("journalEntryRow");
    row.id = "entry" + entry.id;
    row.addEventListener("click", function() {
      openJournalEntry(entry.id);
    })

    let date = document.createElement("td");
    date.innerText = parseJournalDate(entry.dateCreated);
    row.append(date);

    let title = document.createElement("td");
    title.classList.add("journalRowTitle");
    title.innerText = entry.title;
    row.append(title);

    // let content = document.createElement("td");
    // content.classList.add("journalRowContent");
    // content.innerText = entry.htmlContent
    // row.append(content);

    tableBody.prepend(row);
    
  }
}

function parseJournalDate(dateTime) {
  dateTime = dateTime.split(" ");
  let date = dateTime[0];
  let time = dateTime[1];
  let newDate = date.split("-")[1] + "/" + date.split("-")[2];
  let newTime = time.split(":")[0] + ":" + time.split(":")[1];
  dateTime = newDate + " " + newTime;
  return dateTime;

}
let currentID = null;
function openJournalEntry(id) {
  if (currentID != null && hasBeenRecentDelete == false) {
     document.getElementById("entry" + currentID).style.backgroundColor = ""
  }
  hasBeenRecentDelete = false;
  document.getElementById("entry" + id).style.backgroundColor = "#e9f0f9";
  currentID = id;
  for (let i = 0; i < journalData.length; i++) {
    const entry = journalData[i];
    if (entry.id == id) {
      tinymce.get("tinymce-default").setContent(entry.htmlContent);
      tinymce.get('tinymce-default').getBody().setAttribute('contenteditable', true);
    }
    
  }
}

let hasBeenRecentSave = false;

async function saveJournalEdit() {
  let sessionKey = localStorage.getItem("sessionKey");
  let content = tinymce.get("tinymce-default").getContent();
  let id = currentID;
  if (currentID == null) {
    return;
  }
  //console.log(id);
  let bookListID = getBookIDfromURL();
  let title = retreiveTitleFromID(id);

  try {
    const response = await fetch(`/api/journal/${bookListID}/entries/${id}?sessionKey=${sessionKey}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        "title": title,
        "htmlContent": content
      })
    });
    const statusCode = response.status;
    console.log(response);
  } catch (error) {
    console.error(error);
    throw error;
  }
  
  hasBeenRecentSave = true;
  await journalInitHandler();
  openJournalEntry(id);
}

function retreiveTitleFromID(id) {
  for (let i = 0; i < journalData.length; i++) {
    const entry = journalData[i];
    if (entry.id == id) {
      return entry.title;
    }
    
  }
  
  return 0;
}

async function newEntryHandler() {
  let newEntryTitle = document.getElementById("newEntryTitle");
  if (newEntryTitle.value == "") {
    newEntryTitle.classList.add("is-invalid");
    return;
  }
  
  let sessionKey = localStorage.getItem("sessionKey");
  let data = {
    "title": newEntryTitle.value,
    "htmlContent": ""
  }
  try {
    const response = await fetch(`/api/journal/${bookListID}/entries?sessionKey=${sessionKey}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    });
    const statusCode = response.status;
  } catch (error) {
    console.error(error);
    throw error;
  }
  
  hasBeenRecentSave = true;
  journalInitHandler();

}

let hasBeenRecentDelete = false;

async function deleteEntry() {
  let id = currentID;
  let bookListID = getBookIDfromURL();
  let sessionKey = localStorage.getItem("sessionKey");
  try {
    const response = await fetch(`/api/journal/${bookListID}/entries/${id}?sessionKey=${sessionKey}`, {
      method: 'DELETE'
    });
    const statusCode = response.status;
  } catch (error) {
    console.error(error);
    throw error;
  }
  hasBeenRecentSave = true;
  hasBeenRecentDelete = true;
  journalInitHandler();

}