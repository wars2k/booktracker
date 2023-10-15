function updateCardTwo() {
    let challengeType = document.getElementById("challengeTypeSelector").value
    let readingBody = document.getElementById("readingChallengeBody");
    let writingBody = document.getElementById("writingChallengeBody");

    if (challengeType == "writing") {
        readingBody.style.display = "none";
        writingBody.style.display = "block";
        document.getElementById("step4").innerText = "Step 4: Start Writing!"
    } else {
        readingBody.style.display = "block";
        writingBody.style.display = "none";
        document.getElementById("step4").innerText = "Step 4: Start Reading!"
    }
    updateSummary()

    
}

function updateCardTwoQuestion() {
    let startFinishType = document.getElementById("startFinishTypeSelector").value
    let countQuestion = document.getElementById("countQuestion");
    let questionOptions = {
        start: "How many books do you hope to start?",
        finish: "How many books do you hope to finish?"
    }

    if (startFinishType == "1") {
        countQuestion.innerText = questionOptions.start
    } else {
        countQuestion.innerText = questionOptions.finish
    }
    updateSummary()
}

function updateSummary() {
    let challengeType = document.getElementById("challengeTypeSelector").value
    let startFinishType = document.getElementById("startFinishTypeSelector").value
    let challengeSummary = document.getElementById("challengeType");
    let quantity = document.getElementById("challengeQuantity");
    let startDate = document.getElementById("challengeStart");
    let endDate = document.getElementById("challengeEnd");

    if (challengeType == "writing") {
        challengeSummary.innerHTML = "<div class='status status-purple'>Writing</div>";
        quantity.innerText = document.getElementById("num").innerText + " entries";
    } else if (startFinishType == "1") {
        challengeSummary.innerHTML = "<div class='status status-cyan'>Reading - Start Books</div>";
        quantity.innerText = document.getElementById("numTwo").innerText + " books";
    } else {
        challengeSummary.innerHTML = "<div class='status status-cyan'>Reading - Finish Books</div>";
        quantity.innerText = document.getElementById("numTwo").innerText + " books";
    }

    startDate.innerText = document.getElementById("startDate").value;
    endDate.innerText = document.getElementById("endDate").value;
}

function submissionHandler() {
    let challengeType = document.getElementById("challengeTypeSelector").value
    let challengeData = getChallengeData(challengeType)
    let isValid = challengeData.validate()
    if (!isValid) {
        return;
    }
    let jsonChallengeData = getJsonChallengeData(challengeData);
    console.log(jsonChallengeData);
    submitChallengeData(jsonChallengeData)
    window.location.href = "challenges.html";
    
}

function getChallengeData(challengeType) {
    let challengeData = new ChallengeData(challengeType)
    return challengeData;
}

function getJsonChallengeData(challengeData) {
    let json = {};
    console.log(challengeData)
    json.Title = challengeData.title;
    json.Description = challengeData.description;
    json.Type = challengeData.type;
    json.SubType = challengeData.subType;
    json.Start_date = challengeData.startDate;
    json.End_date = challengeData.endDate;
    json.Goal = challengeData.quantity;
    json = JSON.stringify(json);
    return json;
}

async function submitChallengeData(json) {
    try {
        const sessionKey = localStorage.getItem("sessionKey");
        const response = await fetch(`/api/challenges?sessionKey=${sessionKey}`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: json
        });
        const statusCode = response.status;
        window.location.href = "challenges.html";
        return statusCode;
      } catch (error) {
        console.error(error);
        throw error;
      }
}

class ChallengeData {
    title;
    description;
    type;
    subType;
    quantity;
    startDate;
    finishDate;

    constructor(type) {
        this.type = type;

        if (type == "reading") {
            this.subType = document.getElementById("startFinishTypeSelector").value
            this.quantity = document.getElementById("numTwo").value;
        } else {
            this.subType = null;
            this.quantity = document.getElementById("num").value;
        }

        this.startDate = document.getElementById("startDate").value;
        this.endDate = document.getElementById("endDate").value;
        this.title = document.getElementById("challengeTitle").value;
        this.description = document.getElementById("challengeDescription").value;

    }

    validate() {
        let isError = false;
        clearErrors();
        if (this.title == "") {
            showError("title");
            isError = true;
        }
        if (this.startDate == "") {
            showError("startDate");
            isError = true;
        }
        if (this.endDate == "") {
            showError("endDate");
            isError = true;
        }
        let startDate = new Date(this.startDate);
        let endDate = new Date(this.endDate);

        if (startDate >= endDate) {
            showError("date")
            isError = true;
        }

        if (this.quantity <= 0) {
            showError("quantity")
            isError = true;
        }
        if (!isError) {
            return true;
        } else {
            return false;
        }
    }

}

function showError(errorType) {
    if (errorType == "title") {
        document.getElementById("challengeTitle").classList.add("is-invalid");
    }
    if (errorType == "startDate") {
        let startDate = document.getElementById("startDate");
        startDate.classList.add("is-invalid");
    }

    if (errorType == "endDate") {
        let endDate = document.getElementById("endDate");
        endDate.classList.add("is-invalid");
    }

    if (errorType == "date") {
        let startDate = document.getElementById("startDate");
        startDate.classList.add("is-invalid");
        let endDate = document.getElementById("endDate");
        endDate.classList.add("is-invalid");
    }

    if (errorType == "quantity") {
        document.getElementById("num").innerHTML = "<span style='color: red'>Quantity must be greater than one. </span>";
        document.getElementById("numTwo").innerHTML = "<span style='color: red'>Quantity must be greater than one. </span>";
    }
}

function clearErrors() {
    document.getElementById("challengeTitle").classList.remove("is-invalid");
    document.getElementById("startDate").classList.remove("is-invalid");
    document.getElementById("endDate").classList.remove("is-invalid");
}
