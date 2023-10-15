//this is the abstract class
class Challenge {
    id;
    title;
    description;
    dateCreated;
    status;
    type;
    subtype;
    startDate;
    endDate;
    goal;
    count;
    record;
    startDateString;
    endDateString;
    createdDateString;
    card;

    constructor(challengeResponse) {

        this.count = challengeResponse.count;
        this.goal = challengeResponse.goal;
        this.endDate = new Date(challengeResponse.end_date);
        this.startDate = new Date(challengeResponse.start_date);
        this.dateCreated = new Date(challengeResponse.date_created);
        
        this.calculateStatus();
        
        this.startDateString = this.formatDate(challengeResponse.start_date);
        this.endDateString = this.formatDate(challengeResponse.end_date);
        this.createdDateString = this.formatDate(challengeResponse.date_created);

        this.id = challengeResponse.id;
        this.title = challengeResponse.title;
        this.description = challengeResponse.description;
        this.type = challengeResponse.type;
        this.subtype = challengeResponse.subType;
        this.record = JSON.parse(challengeResponse.record);

    }

    //takes in a string formatted 'yyyy-mm-dd [optional extra stuff like time]' and returns it as 'mm/dd/yyyy' for more conventional display. 
    formatDate(date) {
        if (date == null) {
            return "Unknown";
        }
        let dateArray = date.split("-");
        let formattedDate = `${dateArray[1]}/${dateArray[2].slice(0,2)}/${dateArray[0]}`;
        return formattedDate;
    }

    //using count, goal, and endDate, determines and sets the appropriate status for a challenge.
    calculateStatus() {

        let today = new Date();

        if (this.count >= this.goal) {
            this.status = "Complete";
            return;
        }

        if (today > this.endDate) {
            this.status = "Expired";
            return;
        }

        this.status = "Active";
        return 

    }

    calculateMetadataStatus() {
        

        let metaDataStatus = {
            "status": "",
            "color": "",
            "helpText": ""
        }

        let today = new Date();

        if (today > this.endDate) {
            metaDataStatus.status = "N/A";
            metaDataStatus.color = "bg-blue-lt";
            metaDataStatus.helpText = "This challenge has concluded, so it has no progress status.";
            return metaDataStatus
        }

        let totalDays = this.endDate.getTime() - this.startDate.getTime();
        totalDays = totalDays / (1000 * 60 * 60 * 24);

        let daysElapsed = today.getTime() - this.startDate.getTime();
        daysElapsed = daysElapsed / (1000 * 60 * 60 * 24);

        let percentage = daysElapsed/totalDays;

        let progressPercentage = this.count / this.goal;
        if (progressPercentage >= 1) {
            metaDataStatus.status = "N/A";
            metaDataStatus.color = "bg-blue-lt";
            metaDataStatus.helpText = "This challenge has concluded, so it has no progress status.";
            return metaDataStatus
        }

        if (progressPercentage >= percentage) {
            metaDataStatus.status = "On Track";
            metaDataStatus.color = "bg-green-lt";
            metaDataStatus.helpText = `The challenge is ${(percentage * 100).toFixed(2)}% over, and you have completed ${(progressPercentage * 100).toFixed(2)}% of the challenge.`;
        } else {
            metaDataStatus.status = "Behind Schedule";
            metaDataStatus.color = "bg-yellow-lt";
            metaDataStatus.helpText = `The challenge is ${(percentage * 100).toFixed(2)}% over, and you have completed ${(progressPercentage * 100).toFixed(2)}% of the challenge.`;
        }

        return metaDataStatus;

    }

    async buildCard() {
        this.card = document.createElement("div");
        this.card.classList.add("card", "challengeCard");

        let header = this.buildCardHeader();
        this.card.append(header);

        let body = await this.buildCardBody();
        this.card.append(body);
        


    }

    buildCardHeader() {
        //create the header
        let header = document.createElement("div");
        header.classList.add("card-header", "challengeTitleArea");

        //create the title
        let title = document.createElement("h3");
        title.classList.add("card-title");
        title.innerText = this.title;

        //right-side of header
        let headerRightSide = document.createElement("div");
        headerRightSide.classList.add("headerRightSide");

        //status in the right side of the header
        let status = document.createElement("div");
        let color;
        if (this.status == "Active") {
            color = "status-blue"
        } else if (this.status == "Complete") {
            color = "status-green"
        } else if (this.status == "Expired") {
            color = "status-red"
        }
        status.classList.add("status", color);
        status.innerText = this.status;

        //dropdown in the right-side of the header
        let dropdown = document.createElement("div");
        dropdown.classList.add("dropdown");

        dropdown.innerHTML = '<a href="#" class="btn btn-sm btn-ghost-primary dropdown-toggle" data-bs-toggle="dropdown">More</a> \
                            <div class="dropdown-menu"> \
                                <a class="dropdown-item" href="#" onclick="deleteEntry(' + this.id + ')"><button class="btn btn-sm btn-danger">Delete challenge</button></a> \
                            </div>'
        
        //add the two parts of the right-side to the headerRightSide
        headerRightSide.append(status);
        headerRightSide.append(dropdown);
        
        //add the title and right side to the main header
        header.append(title);
        header.append(headerRightSide);

        return header;

    }

    async buildCardBody() {

        let body = document.createElement("div");
        body.classList.add("card-body", "readingCardBody");

        let challengeBody = document.createElement("div");
        challengeBody.classList.add("challengeBody");
        let challengeContent = await this.buildTypeSpecificDisplay();
        //console.log(challengeContent);
        let metadata = this.buildMetadata();
        challengeBody.append(challengeContent, metadata);
        body.append(challengeBody);

        let footer = this.buildFooter();
        body.append(footer);

        return body;



    }

    buildMetadata() {
        let metadataContainer = document.createElement("div");
        metadataContainer.classList.add("challengeMetadataContainer");

        let metadataMain = document.createElement("div");
        metadataMain.classList.add("metaDataMain");

        let table = document.createElement("table");
        table.classList.add("table", "table-responsive");

        let tableBody = document.createElement("tbody");
        let startDate = this.buildMetadataRow("startDate");
        let endDate = this.buildMetadataRow("endDate");
        let status = this.buildMetadataRow("status");
        tableBody.append(startDate, endDate, status);

        table.append(tableBody);
        metadataMain.append(table);

        let progressContainer = document.createElement("div");
        progressContainer.classList.add("progressBar");
        let progress = document.createElement("progress");
        progress.classList.add("progress", "progress-lg");
        progress.value = this.count;
        progress.max = this.goal;
        let progressDescription = document.createElement("div");
        progressDescription.innerHTML = `<b>${this.count}</b> of <b>${this.goal}</b> `;
        if (this.type == "writing") {
            progressDescription.innerHTML += "entries written.";
        } else if (this.subtype == 1) {
            progressDescription.innerHTML += "books started.";
        } else {
            progressDescription.innerHTML += "books finished.";
        }

        progressContainer.append(progress, progressDescription);

        metadataContainer.append(metadataMain, progressContainer);


        return metadataContainer;

    }

    buildMetadataRow(type) {
        let row = document.createElement("tr");
        let name = document.createElement("td");
        let value = document.createElement("td");

        switch (type) {
            case "startDate":
                name.innerText = "Start Date:";
                value.innerText = this.startDateString;
                break;
            case "endDate":
                name.innerText = "End Date:";
                value.innerText = this.endDateString;
                break;
            case "status":
                name.innerText = "Status:";
                let status = this.calculateMetadataStatus();
                value.innerHTML = `<span class="badge ${status.color}" title="${status.helpText}">${status.status}</span>`
            default:
                break;
        }

        row.append(name,value);
        return row;
    }

    buildFooter() {
        let footer = document.createElement("div");
        footer.classList.add("challengeFooter");
        let text = document.createElement("div");
        text.classList.add("text-secondary");
        text.innerText = `ID ${this.id} created ${this.createdDateString}`;

        footer.append(text);
        return footer;
    }

}

class ReadingChallenge extends Challenge {


    constructor(challengeResponse) {
        super(challengeResponse);
        this.buildCard();
    }

    async buildTypeSpecificDisplay() {
        let challengeContent = document.createElement("div");
        challengeContent.classList.add("challengeContent");
        if (this.count == 0) {
            challengeContent.innerHTML = `<div class="empty"> \
            <div class="empty-icon"> \
              <svg xmlns="http://www.w3.org/2000/svg" class="icon" width="24" height="24" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"> \
                <path stroke="none" d="M0 0h24v24H0z" fill="none" /> \
                <circle cx="12" cy="12" r="9" /> \
                <line x1="9" y1="10" x2="9.01" y2="10" /> \
                <line x1="15" y1="10" x2="15.01" y2="10" /> \
                <path d="M9.5 15.25a3.5 3.5 0 0 1 5 0" /> \
              </svg> \
            </div> \
            <p class="empty-subtitle text-secondary">You haven't made any challenge progress yet. Start reading!</p> \
        </div>`
            return challengeContent;
        }

        let table = document.createElement("table");
        table.classList.add("table", "table-responsive");

        let thead = document.createElement("thead");
        thead.classList.add("sticky-top");

        let header = document.createElement("tr");
        let title = document.createElement("th");
        let date = document.createElement("th");

        title.innerText = "Title";
        if (this.subtype == 1) {
            date.innerText = "Date Started";
        } else {
            date.innerText = "Date Finished";
        }

        header.append(title,date);
        thead.append(header);
        table.append(thead);

        let body = document.createElement("tbody");
        body.classList.add("bookTable");
        table.append(body);

        for (let i=0; i < this.record.length; i++) {
            let bookID = this.record[i];
            let data = await this.getBookInfo(bookID);
            //console.log(data);
            let row = document.createElement("tr");
            row.classList.add("test");
            let title = document.createElement("td");
            let dateFinished = document.createElement("td");

            title.innerHTML = `<a href='bookPage.html?bookListId=${bookID}'>${data.title}</a>`;
            if (this.subtype == 1) {
                dateFinished.innerText = this.formatDate(data.dateStarted)
            } else {
                dateFinished.innerText = this.formatDate(data.dateFinished);
            }

            row.append(title,dateFinished);
            body.append(row);
            

        }

        challengeContent.append(table);
        //console.log(challengeContent);
        return challengeContent;
    }

    async getBookInfo(bookListID) {
        let sessionKey = localStorage.getItem("sessionKey");
      
        return fetch(`/api/Booklist/${bookListID}/data?sessionKey=${sessionKey}`, {
          method: 'GET',
      })
      .then(response => {
          if (response.status === 401) {
              informIncorrectPassword()
          }
          //console.log("TEST");
          return response.json()
      })
      .then(data => {
        //console.log("TEST2")
        return data;
      })
      .catch(error => console.error(error));
    }
}

class WritingChallenge extends Challenge {

    constructor(challengeResponse) {
        super(challengeResponse);
        this.buildCard();
    }

    async buildTypeSpecificDisplay() {
        let challengeContent = document.createElement("div");
        challengeContent.classList.add("challengeContent");
        if (this.count == 0) {
            challengeContent.innerHTML = `<div class="empty"> \
            <div class="empty-icon"> \
              <svg xmlns="http://www.w3.org/2000/svg" class="icon" width="24" height="24" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round"> \
                <path stroke="none" d="M0 0h24v24H0z" fill="none" /> \
                <circle cx="12" cy="12" r="9" /> \
                <line x1="9" y1="10" x2="9.01" y2="10" /> \
                <line x1="15" y1="10" x2="15.01" y2="10" /> \
                <path d="M9.5 15.25a3.5 3.5 0 0 1 5 0" /> \
              </svg> \
            </div> \
            <p class="empty-subtitle text-secondary">You haven't made any challenge progress yet. Start reading!</p> \
        </div>`
            return challengeContent;
        }

        let table = document.createElement("table");
        table.classList.add("table", "table-responsive");

        let thead = document.createElement("thead");
        thead.classList.add("sticky-top");

        let header = document.createElement("tr");
        let title = document.createElement("th");
        let date = document.createElement("th");

        title.innerText = "Entry Title";
        date.innerText = "Date Written";

        header.append(title,date);
        thead.append(header);
        table.append(thead);

        let body = document.createElement("tbody");
        body.classList.add("bookTable");
        table.append(body);

        for (let i=0; i < this.record.length; i++) {
            let journalID = this.record[i];
            let data = await this.getEntryInfo(journalID);
            //console.log(data);
            let row = document.createElement("tr");
            row.classList.add("test");
            let title = document.createElement("td");
            let dateWritten = document.createElement("td");

            title.innerHTML = `<a href="bookPage.html?bookListId=${data.idBookList}&journalID=${data.id}">${data.title}</a>`;
            dateWritten.innerText = this.formatDate(data.dateCreated);

            row.append(title,dateWritten);
            body.append(row);
            

        }

        challengeContent.append(table);
        //console.log(challengeContent);
        return challengeContent;
    }

    async getEntryInfo(journalID) {
        let sessionKey = localStorage.getItem("sessionKey");
      
        return fetch(`/api/journal/${journalID}?sessionKey=${sessionKey}`, {
          method: 'GET',
      })
      .then(response => {
          if (response.status === 401) {
              informIncorrectPassword()
          }
          //console.log("TEST");
          return response.json()
      })
      .then(data => {
        //console.log("TEST2")
        return data;
      })
      .catch(error => console.error(error));
    }
}
