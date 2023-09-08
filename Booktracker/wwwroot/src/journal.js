//journal section

let journalData
let currentID = null;
let hasBeenRecentSave = false;
let hasBeenRecentDelete = false;

function showJournalPane() {
  document.getElementById("journalEditorPane").style.display = "flex";
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

function openJournalEntry(id) {
  if (currentID != null && hasBeenRecentDelete == false) {
     document.getElementById("entry" + currentID).style.backgroundColor = ""
  }
  hasBeenRecentDelete = false;
  document.getElementById("entry" + id).style.backgroundColor = "#e9f0f9";
  currentID = id;
  document.getElementById("saveEntryButton").style.display = "inline";
  document.getElementById("deleteEntryButton").style.display = "inline";
  for (let i = 0; i < journalData.length; i++) {
    const entry = journalData[i];
    if (entry.id == id) {
      tinymce.get("tinymce-default").setContent(entry.htmlContent);
      tinymce.get('tinymce-default').getBody().setAttribute('contenteditable', true);
    }
    
  }

  document.getElementById("tinymce-default").addEventListener("input", function() {
    console.log("input event fired");
  }, false);

}



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
  document.getElementById("saveEntryButton").classList.add("disabled");
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
  openNewEntry();

}

function openNewEntry() {
    openJournalEntry(journalData[journalData.length - 1].id)
}



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

function checkForSave() {
  let currentContent = tinymce.get("tinymce-default").getContent();
  if (currentID == null) {
    return;
  }
  let savedContent
  for (let i = 0; i < journalData.length; i++) {
    const entry = journalData[i];
    if (entry.id == currentID) {
      savedContent = entry.htmlContent;
      break;
    }
    
  }
  if (currentContent == savedContent) {
    document.getElementById("saveEntryButton").classList.add("disabled");
    return;
  } else {
    document.getElementById("saveEntryButton").classList.remove("disabled");
    return;
  }
}