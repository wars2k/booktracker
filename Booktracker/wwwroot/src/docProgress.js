//Calling this so that the journal select is populated on load. 
//This is also called anytime a new journal entry is created in journal.js.
fillJournalEntries()

/**
 * True if the current page will equal the last page of the book. This
 * affects whether or not we update status and date finished. 
 */
let isLastPage = false;

/**
 * True if Linked Journal Entry is set to Create New. This is used to
 * make sure we create the journal entry before creating the progress update.
 */
let shouldCreateJournal = false;

/**
 * Called when the currentPage field changes. Validates the input based on the book's
 * page count and also builds dateFinished row if documenting progress on last page.
 * @returns 0 if invalid. 
 */
function validateCurrentPage(isSubmission) {
    

    let input = document.getElementById("currentPage");
    input.classList.remove("is-invalid");
    let currentEntry = input.value;

    if (!isSubmission) {
        //reset to false and only change to true if conditions are met. 
        isLastPage = false;

        let div = document.getElementById("extraPageElements");
        div.innerHTML = "";
    }
    
    if (parseInt(currentEntry) > parseInt(globalPageCount)) {
        input.classList.add("is-invalid");
        return 0;
    }

    if (parseInt(currentEntry) <= 0) {
        input.classList.add("is-invalid");
        return 0;
    }

    if (parseInt(currentEntry) == parseInt(globalPageCount) && !isSubmission) {
        isLastPage = true;
        buildLastPageRows();
        
    }


}

/**
 * Called when "Last Page" quick button is pressed. Automatically fills in
 * the book's last page in the Current Page field.
 */
function inputLastPage() {
    let input = document.getElementById("currentPage");
    input.value = parseInt(globalPageCount);
    isLastPage = true;
    buildLastPageRows();
}

/**
 * Builds the "Date Finished" field. Only appears when Current Page == page count.
 */
function buildLastPageRows() {
    let div = document.getElementById("extraPageElements");
    div.innerHTML = "";
    let dateFinished = document.createElement("div");
    dateFinished.classList.add("mb-3");
    let details = document.createElement("p");
    details.innerHTML = "Progress that ends on a book's last page will automatically change the status to <span class='badge bg-purple-lt'>FINISHED</span>. Please enter the date the book was finished."

    let input = document.createElement("input");
    input.classList.add("form-control");
    input.style.marginRight = "10px";
    input.type = "date";
    input.id = "date";
    if (globalFinishedDate == null) {
        input.value = new Date().toLocaleDateString('sv');
    } else {
        input.value = globalFinishedDate;
    }
    
    
    
    let label = document.createElement("label");
    label.classList.add("form-label", "required");
    label.innerHTML = "Date Finished";

    dateFinished.append(label);
    dateFinished.append(details);
    dateFinished.append(input);

    div.append(dateFinished);

}

/**
 * Uses functionality from journal.js to get journal data, then creates an option
 * in the Linked Journal Entry drop-down for each jourrnal entry. 
 */
async function fillJournalEntries() {
    let bookListId = getBookIDfromURL();
    let data = await getJournalData(bookListId);
    let journalSelect = document.getElementById("journalSelect");

    journalSelect.innerHTML = '<option value="none">None</option> \
    <option value="new">Create New Journal Entry</option>'

    for (let i = 0; i < data.length; i++) {
        const entry = data[i];
        let selectOption = document.createElement("option");
        selectOption.value = entry.id;
        selectOption.innerText = entry.title;
        journalSelect.append(selectOption);   
    }
}

/**
 * Called anytime the Linked Journal Entry field changes. If it's chaanged to "new",
 * then a new field is created that prompts the user for a new journal entry title.
 * @returns null
 */
function updateJournalQuestions() {
    let journalSelect = document.getElementById("journalSelect");
    let journalSection = document.getElementById("journalSection");
    journalSection.innerHTML = "";
    if (journalSelect.value != "new") {
        shouldCreateJournal = false;
        return;
    }

    shouldCreateJournal = true;

    let container = document.createElement("div");
    container.classList.add("mb-3");

    let label = document.createElement("label");
    label.classList.add("form-label", "required");
    label.innerText = "Journal Title";
    container.append(label);

    let title = document.createElement("input");
    title.type = "text";
    title.id = "titleInput";
    title.classList.add("form-control");
    container.append(title);

    journalSection.append(container);
}

/**
 * Called when the user clicks the submit button on the document progress form.
 * Validates the submission, then makes status/date updates, creates journal entries
 * where necessary, and submits progress data to the server. Reloads the page after. 
 * @returns null
 */
async function submissionHandler() {
    let isValid = validateSubmission();
    if (!isValid) {
        return;
    }

    let submissionData = gatherSubmissionData()

    if (isLastPage) {
        lastPageSubmissionHandler(submissionData.dateFinished);
    }

    if (shouldCreateJournal) {
        submissionData.journal = await journalCreationHandler(submissionData.entryTitle);
    } else {
        //submissionData.journal = null;
    }

    await submitProgress(submissionData);

    //location.reload();
}

/**
 * Evaluates the reading progress submission form and highlights errors.
 * @returns True for valid submissions, false otherwise.
 */
function validateSubmission() {
    let isValid = true;

    let currentPage = document.getElementById("currentPage");
    currentPage.classList.remove("is-invalid");
    validateCurrentPage(true);

    if (currentPage.value == "") {
        currentPage.classList.add("is-invalid");
        isValid = false;
    }

    if (isLastPage) {
        let dateEntry = document.getElementById("date");
        dateEntry.classList.remove("is-invalid");

        if (dateEntry.value == "") {
            dateEntry.classList.add("is-invalid");
            isValid = false;
        }

    }

    if (shouldCreateJournal) {
        let title = document.getElementById("titleInput");
        title.classList.remove("is-invalid");

        if (title.value == "") {
            title.classList.add("is-invalid");
            isValid = false;
        }
    }

    return isValid;
}

/**
 * Gathers Reading Progress submission data and conditionally builds an object
 * to store it. 
 * @returns An object holding all Reading Progress submission data. 
 */
function gatherSubmissionData() {
    let data = {};
    
    data.currentPage = document.getElementById("currentPage").value;

    if (isLastPage) {
        data.dateFinished = document.getElementById("date").value;
    }
    
    let linkedJournal = document.getElementById("journalSelect");
    if (linkedJournal.value != "none" && linkedJournal.value != "new") {
        data.journal = linkedJournal.value;
    }

    if (shouldCreateJournal) {
        data.entryTitle = document.getElementById("titleInput").value;
    }

    data.comment = document.getElementById("progressComment").value;

    return data;
}

/**
 * Submits a bookList update that changes status to "FINISHED" and updates the finish date
 * to the user-provided date. 
 * @param {*} dateFinished The date finished that the user provides in the reading progress form.
 */
function lastPageSubmissionHandler(dateFinished) {
    let body = {};
    body.sessionKey = localStorage.getItem("sessionKey");

    let data = {
        "id": getBookIDfromURL(),
        "rating": null,
        "status": "FINISHED",
        "startDate": null,
        "finishedDate": dateFinished
    };

    if (globalStatus == "FINISHED") {
        data.status = null;
    }

    body.data = data;
    
    fetch(`/api/BookList/${body.data.id}`, {
        method: 'PUT',
        body: JSON.stringify(body),
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (response.status === 401) {
            console.log("ERROR");
        }
        return
    })
    .then(data => {
      //console.log(data);
      
    })
    .catch(error => console.error(error));
    

}

/**
 * Creates an empty journal entry.
 * @param {string} title The title of the journal entry that the user enters.
 */
async function journalCreationHandler(title) {
    let sessionKey = localStorage.getItem("sessionKey");
    let bookListID = getBookIDfromURL();
    let data = {
        "title": title,
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
        const responseData = await response.json();
        return responseData;
    } catch (error) {
        console.error(error);
        throw error;
    }
}

/**
 * Submits the progress update to the server. 
 * @param {Object} data Data pulled from gatherSubmissionData(). 
 */
async function submitProgress(data) {
    let sessionKey = localStorage.getItem("sessionKey");
    let bookListID = getBookIDfromURL();
    let body = {
        "currentPosition": data.currentPage,
        "journal": data.journal,
        "comment": data.comment
    }
    if (body.comment == "") {
        body.comment = null;
    }
    
    try {
        const response = await fetch(`/api/BookList/${bookListID}/progress?sessionKey=${sessionKey}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(body)
        });
        const statusCode = response.status;
    } catch (error) {
        console.error(error);
        throw error;
    }
}