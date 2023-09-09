//holds journal data after it is retrieved from the server
let journalData

//current ID of the journal entry being viewed.
let currentID = null;

//determines if there has been a recent save or delete. 
//Helps determine if journalInithandler should get the data from the server again or not.
let hasBeenRecentSave = false;
let hasBeenRecentDelete = false;

/**
 * Makes the journal editor pane & the Add Entry button appear. Also hides the other buttons until an entry is selected.
 */
function showJournalPane() {
  document.getElementById("journalEditorPane").style.display = "flex";
  document.getElementById("addEntryButton").style.display = "inline";
  document.getElementById("deleteEntryButton").style.display="none";
  document.getElementById("saveEntryButton").style.display="none";
}

/**
 * Hides the journal editor pane and all associated buttons.
 */
function hideJournalPane() {
  currentID = null;
  document.getElementById("journalEditorPane").style.display = "none";
  document.getElementById("saveEntryButton").style.display = "none";
  document.getElementById("deleteEntryButton").style.display = "none";
  document.getElementById("addEntryButton").style.display = "none";
}

/**
 * Sets up the journal environment: gets entry data, builds entry table, and shows editor pane.
 */
async function journalInitHandler() {

  let data;
  let bookListId = getBookIDfromURL();

  //show the journal pane, and make it read only until an entry is selected.
  showJournalPane()
  tinymce.get("tinymce-default").setContent("Choose an entry to the right to start editing!");
  tinymce.get('tinymce-default').getBody().setAttribute('contenteditable', false);

  //if we don't have any journalData OR there's been a recent save
  //we need to get the data again.
  if (journalData == null || hasBeenRecentSave) {
    data = await getJournalData(bookListId);
    journalData = data;
  } else {
    data = journalData;
  }

  buildJournalTable(data);

}

/**
 * Gets journal entry data from the server and returns it as an array of objects.
 * @param {string} id - The bookList ID that will be provided to the server for retrieving journal entries.
 * @returns - An array of journal entry data from the server.
 */
function getJournalData(id) {
  let sessionKey = localStorage.getItem("sessionKey");
      
      return fetch(`/api/journal/${id}/entries?sessionKey=${sessionKey}`, {
          method: 'GET',
      })
      .then(response => {
          return response.json()
      })
      .then(data => {
        return data;
      })
      .catch(error => console.error(error));
}

/**
 * Builds the table of journal entries that appears on the right-side of the user's screen.
 * @param {object} data - The json journal data from the server in object-form.
 */
function buildJournalTable(data) {
  let tableBody = document.getElementById("journalEntryTableBody")
  tableBody.innerHTML = "";

  //for each entry, create a row and add the date & title to it.
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

    tableBody.prepend(row);
  }
}

/**
 * Formats the C# dateTime into something more readable.
 * @param {string} dateTime - A datetime string in the form of '10-02-23 09:58'
 * @returns - The same date formatted like 'MM/DD HH:MM'
 */
function parseJournalDate(dateTime) {
  dateTime = dateTime.split(" ");
  let date = dateTime[0];
  let time = dateTime[1];
  let newDate = date.split("-")[1] + "/" + date.split("-")[2];
  let newTime = time.split(":")[0] + ":" + time.split(":")[1];
  dateTime = newDate + " " + newTime;
  return dateTime;

}

/**
 * Opens the provided journal entry, highlights the correct entry on the table, and displays the save/delete buttons.
 * @param {string} id - The journal entry ID that is to be dispalyed.
 */
function openJournalEntry(id) {

  //if there's a current entry highlighted, unhighlight it.
  if (currentID != null && hasBeenRecentDelete == false) {
     document.getElementById("entry" + currentID).style.backgroundColor = ""
  }
  currentID = id;
  hasBeenRecentDelete = false;

  //highlight the current entry and display the save/detele buttons
  document.getElementById("entry" + id).style.backgroundColor = "#e9f0f9";
  document.getElementById("saveEntryButton").style.display = "inline";
  document.getElementById("deleteEntryButton").style.display = "inline";

  //display the saved content and make the journal editable
  let content = retrieveInfoFromID(id, "content");
  tinymce.get("tinymce-default").setContent(content);
  tinymce.get('tinymce-default').getBody().setAttribute('contenteditable', true);

}


/**
 * Saves a recently-edited journal entry to the server.
 * @returns Null
 */
async function saveJournalEdit() {
  let sessionKey = localStorage.getItem("sessionKey");
  let content = tinymce.get("tinymce-default").getContent();
  let id = currentID;
  if (currentID == null) {
    return;
  }
  //console.log(id);
  let bookListID = getBookIDfromURL();
  let title = retrieveInfoFromID(id,"title");

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
  } catch (error) {
    console.error(error);
    throw error;
  }
  
  hasBeenRecentSave = true;
  await journalInitHandler();
  document.getElementById("saveEntryButton").classList.add("disabled");
  openJournalEntry(id);
}

/**
 * A helper function that gets info for a given journal entry ID from the local array.
 * @param {string} id - The journal entry ID.
 * @param {string} infoType What will be returned. Options are "title" or "content"
 * @returns Either the title or the html content of the requested entry.
 */
function retrieveInfoFromID(id,infoType) {
  for (let i = 0; i < journalData.length; i++) {
    const entry = journalData[i];
    if (entry.id == id) {
      if (infoType=="title") {
        return entry.title;
      } else {
        return entry.htmlContent;
      }
      
    }
    
  }
  return 0;
}

/**
 * Handles new entry workflow. Validates new title, then sends the new entry to the server to be saved.
 * @returns Null
 */
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
  await journalInitHandler();
  openNewEntry();

}

function openNewEntry() {
    openJournalEntry(journalData[journalData.length - 1].id)
}


/**
 * Deletes the currently selected journal entry.
 */
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

/**
 * Checks if the current entry is equal to the saved entry. If it isn't enable the save the button.
 * @returns Null
 */
function checkForSave() {
  let currentContent = tinymce.get("tinymce-default").getContent();
  if (currentID == null) {
    return;
  }
  let savedContent = retrieveInfoFromID(currentID,"entry");
  if (currentContent == savedContent) {
    document.getElementById("saveEntryButton").classList.add("disabled");
    return;
  } else {
    document.getElementById("saveEntryButton").classList.remove("disabled");
    return;
  }
}