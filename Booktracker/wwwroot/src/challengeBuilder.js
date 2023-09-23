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
        quantity.innerText = document.getElementById("num").innerText;
    } else if (startFinishType == "1") {
        challengeSummary.innerHTML = "<div class='status status-cyan'>Reading - Start Books</div>";
        quantity.innerText = document.getElementById("numTwo").innerText;
    } else {
        challengeSummary.innerHTML = "<div class='status status-cyan'>Reading - Finish Books</div>";
        quantity.innerText = document.getElementById("numTwo").innerText;
    }

    startDate.innerText = document.getElementById("startDate").value;
    endDate.innerText = document.getElementById("endDate").value;
}

