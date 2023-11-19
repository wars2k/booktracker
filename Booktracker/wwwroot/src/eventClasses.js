/**
 * Abstract class for events. Defines the basic properties and methods
 * needed to display events on the client. Child classes must have getBadge(),
 * getMainInfo(), and getSecondaryInfo() implemented for buildRow() to work correctly.
 */
class BookEvent {

    //these are what we get from the server
    id;
    userID;
    bookListID;
    dateTime;
    eventType;
    value;

    //these are what we need to create
    row;
    dateObject;
    dateString;
    timeString;

    constructor(eventResponse) {
        this.id = eventResponse.id;
        this.userID = eventResponse.userID;
        this.bookListID = eventResponse.bookListID;
        this.dateTime = eventResponse.dateTime;
        this.eventType = eventResponse.eventType;
        this.value = eventResponse.value;

        this.formatDate();
    }

    /**
     * For the dateTime provided by the API response, this method
     * creates a dateObject and a formatted date and time string
     * to be used on the events table.
     */
    formatDate() {
        let dateTimeArray = this.dateTime.split(" ");

        let dateArray = dateTimeArray[0].split("-");
        let formattedDate = `${dateArray[1]}/${dateArray[2].slice(0,2)}/${dateArray[0]}`;
        this.dateString = formattedDate;

        this.dateObject = new Date(dateArray[0], dateArray[1] - 1, dateArray[2].slice(0,2));

        let time = dateTimeArray[1];
        let timeArray = time.split(".");
        this.timeString = timeArray[0];
    }

    /**
     * Calls all necessary functions to build an HTML row containing the
     * event info. Requires getBadge(), getMainInfo(), and getSecondaryInfo()
     * to be implemented to correctly return a row. 
     */
    async buildRow() {
        let row = document.createElement("tr");
        row.classList.add("event");

        let timestamp = this.buildTimestamp();
        let info = await this.buildInfo();

        row.append(timestamp, info);
        this.row = row;
    }

    /**
     * Builds and returns the timestamp cell for a given event.
     * @returns A "td" element that displays timestamp and event type
     */
    buildTimestamp() {
        let timestamp = document.createElement("td");
        timestamp.classList.add("timestamp");

        let time = document.createElement("p");
        time.classList.add("text-secondary");
        time.innerText = this.timeString;

        let badge = this.getBadge();

        timestamp.append(time, badge);

        return timestamp;
    }

    /**
     * Builds and returns the "info" cell for a given event.
     * @returns A "td" element that displays the main event sentence and optional secondary info.
     */
    async buildInfo() {
        let info = document.createElement("td");
        info.classList.add("info");

        let mainInfo = this.getMainInfo();
        let secondaryInfo = await this.getSecondaryInfo();

        info.append(mainInfo);
        if (secondaryInfo != null) {
            info.append(secondaryInfo);
        }

        return info;
    }

    /**
     * Used by StartDateEvent & FinishDateEvent, this takes in
     * a date formatted yyyy-mm-dd and returns it as mm/dd/yyyy
     * @param {string} date Date formatted in yyyy-mm-dd
     * @returns 
     */
    formatSmallDate(date) {
        if (date == null) {
            return "Unknown";
        }
        let dateArray = date.split("-");
        let formattedDate = `${dateArray[1]}/${dateArray[2].slice(0,2)}/${dateArray[0]}`;
        return formattedDate;
    }


}

/**
 * Class to be used for booktracker events with a status of "statusUpdate".
 * async buildRow() method should be called after initializing object.
 */
class StatusEvent extends BookEvent {

    statusHTML = {
        "DNF": 'Status updated to <span class="badge bg-red-lt">DNF</span>.',
        "UNASSIGNED": 'Status updated to <span class="badge bg-yellow-lt">UNASSIGNED</span>.',
        "READING": 'Status updated to <span class="badge bg-green-lt">READING</span>.',
        "UP NEXT": 'Status updated to <span class="badge bg-teal-lt">UP NEXT</span>.',
        "TO READ": 'Status updated to <span class="badge bg-azure-lt">TO READ</span>.',
        "WISHLIST": 'Status updated to <span class="badge bg-blue-lt">WISHLIST</span>.',
        "FINISHED": 'Status updated to <span class="badge bg-purple-lt">FINISHED</span>.'
    }

    constructor(eventResponse) {
        super(eventResponse);
    }

    getBadge() {
        let badge = document.createElement("span");
        badge.classList.add("badge", "bg-blue-lt");
        badge.innerText = "Status";
        return badge;
    }

    getMainInfo() {
        let mainInfo = document.createElement("p");
        mainInfo.innerHTML = this.statusHTML[this.value];
        return mainInfo;
    }

    getSecondaryInfo() {
        return null;
    }

}

/**
 * Class to be used for booktracker events with a status of "ratingUpdate".
 * async buildRow() method should be called after initializing object.
 */
class RatingEvent extends BookEvent {

    ratingHTML = {
        1: 'Rating updated to <span class="badge bg-red-lt">&#9734</span>.',
        2: 'Rating updated to <span class="badge bg-orange-lt">&#9734&#9734</span>.',
        3: 'Rating updated to <span class="badge bg-yellow-lt">&#9734&#9734&#9734</span>.',
        4: 'Rating updated to <span class="badge bg-blue-lt">&#9734&#9734&#9734&#9734</span>.',
        5: 'Rating updated to <span class="badge bg-green-lt">&#9734&#9734&#9734&#9734&#9734</span>.'
    }

    constructor(eventResponse) {
        super(eventResponse);
    }

    getBadge() {
        let badge = document.createElement("span");
        badge.classList.add("badge", "bg-green-lt");
        badge.innerText = "Rating";
        return badge;
    }

    getMainInfo() {
        let mainInfo = document.createElement("p");
        mainInfo.innerHTML = this.ratingHTML[this.value];
        return mainInfo;
    }

    getSecondaryInfo() {
        return null;
    }

}

/**
 * Class to be used for booktracker events with a status of "dateStartedUpdate".
 * async buildRow() method should be called after initializing object.
 */
class StartDateEvent extends BookEvent {

    constructor(eventResponse) {
        super(eventResponse);
    }

    getBadge() {
        let badge = document.createElement("span");
        badge.classList.add("badge", "bg-cyan-lt");
        badge.innerText = "Start Date";
        return badge;
    }

    getMainInfo() {
        let mainInfo = document.createElement("p");
        mainInfo.innerHTML = "Start date updated to <b>" + this.formatSmallDate(this.value) + "</b>.";
        return mainInfo;
    }

    getSecondaryInfo() {
        return null;
    }

}

/**
 * Class to be used for booktracker events with a status of "dateFinishedUpdate".
 * async buildRow() method should be called after initializing object.
 */
class FinishDateEvent extends BookEvent {

    constructor(eventResponse) {
        super(eventResponse);
    }

    getBadge() {
        let badge = document.createElement("span");
        badge.classList.add("badge", "bg-cyan-lt");
        badge.innerText = "Finish Date";
        return badge;
    }

    getMainInfo() {
        let mainInfo = document.createElement("p");
        mainInfo.innerHTML = "Finished date updated to <b>" + this.formatSmallDate(this.value) + "</b>.";
        return mainInfo;
    }

    getSecondaryInfo() {
        return null;
    }

}

/**
 * Class to be used for booktracker events with a status of "added".
 * async buildRow() method should be called after initializing object.
 */
class AddedEvent extends BookEvent {

    constructor(eventResponse) {
        super(eventResponse);
    }

    getBadge() {
        let badge = document.createElement("span");
        badge.classList.add("badge", "bg-grey-lt");
        badge.innerText = "Added";
        return badge;
    }

    getMainInfo() {
        let mainInfo = document.createElement("p");
        mainInfo.innerHTML = "Book added to book list.";
        return mainInfo;
    }

    getSecondaryInfo() {
        return null;
    }

}

/**
 * Class to be used for booktracker events with a status of "journal".
 * async buildRow() method should be called after initializing object.
 */
class JournalEvent extends BookEvent {

    constructor(eventResponse) {
        super(eventResponse);
    }

    getBadge() {
        let badge = document.createElement("span");
        badge.classList.add("badge", "bg-purple-lt");
        badge.innerText = "Journal";
        return badge;
    }

    getMainInfo() {
        let mainInfo = document.createElement("p");
        mainInfo.innerHTML = "Journal entry created.";
        return mainInfo;
    }

    /**
     * Creates an html "p" element that contains the journal's title with a link to open it.
     * If that entry has been deleted, returns a "p" elemenet that indicates so.
     * @returns An html "p" element.
     */
    async getSecondaryInfo() {
        let title = document.createElement("p");
        title.classList.add("text-secondary", "event-details");
        let titleContent = await this.getJournalTitle()
        if (titleContent.includes("<code>")) {
            title.innerHTML = titleContent;
        } else {
            title.innerHTML = `<a href="bookPage.html?bookListId=${getBookIDfromURL()}&journalID=${this.value}">${titleContent}</a>`
        }
        return title;
    }

    async getJournalTitle() {
        let data = await this.getJournalData();
        if (data == "deleted entry") {
            return "<code>This entry has been deleted.</code>"
        }
        return data.title;
    }

    async getJournalData() {
        let sessionKey = localStorage.getItem("sessionKey");
      
        return fetch(`/api/journal/${this.value}?sessionKey=${sessionKey}`, {
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


}

/**
 * Class to be used for booktracker events with a status of "progress".
 * async buildRow() method should be called after initializing object.
 */
class ProgressEvent extends BookEvent {
    
    constructor(eventResponse) {
        super(eventResponse);
    }

    getBadge() {
        let badge = document.createElement("span");
        badge.classList.add("badge", "bg-lime-lt");
        badge.innerText = "Progress";
        return badge;
    }

    getMainInfo() {
        let mainInfo = document.createElement("p");
        mainInfo.innerHTML = "Progress event created.";
        return mainInfo;
    }

    /**
     * Creates a div and fills it with a progress element showing the user's
     * current progress with the book as well as a "p" element that is only 
     * added if the user added a comment and/or linked a journal entry to this progress event.
     * @returns A div with a progress element and an optional p element.
     */
    async getSecondaryInfo() {
        let details = document.createElement("div");
        let progressResponse = await this.getProgressData();

        let progressContainer = document.createElement("div");
        progressContainer.style.display = "flex";
        progressContainer.style.alignItems = "center";
        progressContainer.style.gap = "10px";

        let progressBar = document.createElement("progress");
        progressBar.classList.add("progress", "progress-lg");
        progressBar.style.maxWidth = "250px";
        progressBar.max = globalPageCount;
        progressBar.value = progressResponse.currentPosition;
        progressContainer.append(progressBar);

        let numbers = document.createElement("p");
        numbers.classList.add("text-secondary", "event-details");
        numbers.innerText = progressResponse.currentPosition + "/" + globalPageCount
        progressContainer.append(numbers);

        details.append(progressContainer);

        if (progressResponse.comment != null) {
            let comment = document.createElement("div");
            comment.classList.add("text-secondary", "event-details");
            comment.innerHTML = "<b>Comment: </b>" + progressResponse.comment;
            details.append(comment);
        }

        if (progressResponse.journal != null) {
            let journalSection = await this.getJournalInfo(progressResponse.journal);
            details.append(journalSection);
        }

        return details;
        
    }

    async getProgressData() {
        let sessionKey = localStorage.getItem("sessionKey");
      
        return fetch(`/api/BookList/${getBookIDfromURL()}/progress/${this.value}?sessionKey=${sessionKey}`, {
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
     * Creates an html "p" element that contains the journal's title with a link to open it.
     * If that entry has been deleted, returns a "p" elemenet that indicates so.
     * @returns An html "p" element.
     */
    async getJournalInfo(journalID) {
        let title = document.createElement("p");
        title.classList.add("text-secondary", "event-details");
        let titleContent = await this.getJournalTitle(journalID)
        if (titleContent.includes("<code>")) {
            title.innerHTML = titleContent;
        } else {
            title.innerHTML = `<b>Linked Journal: </b> <a href="bookPage.html?bookListId=${getBookIDfromURL()}&journalID=${journalID}">${titleContent}</a>`
        }
        return title;
    }

    async getJournalTitle(journalID) {
        let data = await this.getJournalData(journalID);
        if (data == "deleted entry") {
            return "<code>This entry has been deleted.</code>"
        }
        return data.title;
    }

    async getJournalData(journalID) {
        let sessionKey = localStorage.getItem("sessionKey");
      
        return fetch(`/api/journal/${journalID}?sessionKey=${sessionKey}`, {
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

}

let test = {
    "id": 10,
    "userID": 1,
    "bookListID": 3,
    "dateTime": "2023-10-20 22:24:28.3628429",
    "eventType": "ratingUpdate",
    "value": 5
}

async function letsdoit() {
    let object = new JournalEvent(test);
    await object.buildRow();
    console.log(object);
    console.log(object.dateString);
    console.log(object.row);
    document.getElementById("eventTable").prepend(object.row);
}

//letsdoit();
