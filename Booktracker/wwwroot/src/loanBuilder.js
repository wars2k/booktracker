let globalBookListID

init();

async function init() {
    fillLoanTitle();
    await buildLoaneeList();
}

async function fillLoanTitle() {
    let titleInput = document.getElementById("bookTitle");
    let bookListId = getBookIDfromURL();
    let data = await getBookData(bookListId);
    titleInput.value = data.title;

    
}

function getBookIDfromURL() {
    let urlParams = new URLSearchParams(window.location.search);
    bookListID = urlParams.get('bookListId');
    globalBookListID = bookListID;
    return bookListID
}

async function getBookData(id) {
    let sessionKey = localStorage.getItem("sessionKey");
      
      return fetch(`/api/Booklist/${id}/data?sessionKey=${sessionKey}`, {
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
      .catch(error => console.error(error));
}

function handleLoaneeChange() {
    let input = document.getElementById("loanee");

    if (input.value != "new") {
        document.getElementById("loaneeInfo").style.display = "none";
    } else {
        document.getElementById("loaneeInfo").style.display = "";
    }
}

async function buildLoaneeList() {
    let loanees = await getLoanees();
    
    for (let loanee of loanees) {
        
        addLoaneeToList(loanee);
    }
}

async function getLoanees() {
    let sessionKey = localStorage.getItem("sessionKey");
      
      return fetch(`/api/loans/loanees?sessionKey=${sessionKey}`, {
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

function addLoaneeToList(loanee) {
    let list = document.getElementById("loanee");

    let element = document.createElement("option");
    element.innerText = loanee.name;
    element.value = loanee.id;
    list.append(element);
}


async function handleSubmission() {
    if (!isValidSubmission()) {
        return;
    }

    let loanee = document.getElementById("loanee");

    let body = {
        "bookListID": parseInt(globalBookListID),
        "loaneeID": 0,
        "date": null,
        "returnDate": null,
        "comment": null
    };
    
    
    if (document.getElementById("loanDate").value != "") {
        body.date = document.getElementById("loanDate").value;
    }

    if (document.getElementById("loanComment").value != "") {
        body.date = document.getElementById("loanComment").value;
    }

    if (loanee.value == "new") {
        
        let loaneeID = await createLoanee();
        
        body.loaneeID = parseInt(loaneeID)
    } else {
        
        body.loaneeID = parseInt(loanee.value);
    }

    await submitNewLoan(body);
    window.location.href = "loans.html"

    
}

async function submitNewLoan(body) {
    let sessionKey = localStorage.getItem("sessionKey");
      
      return fetch(`/api/loans?sessionKey=${sessionKey}`, {
          method: 'POST',
          body: JSON.stringify(body),
          headers: {
            'Content-Type': 'application/json'
          }
      })
      .then(response => {
          return response.json()
      })
      .then(data => {
        return data;
      })
      .catch(error => console.error(error));
}

function isValidSubmission() {
    let isValid = 1;
    let title = document.getElementById("bookTitle");
    if (title.value == "") {
        title.classList.add("is-invalid");
        isValid = 0;
    }

    let loaneeInput = document.getElementById("loanee");
    if (loaneeInput.value == "") {
        loaneeInput.classList.add("is-invalid");
        isValid = 0;
    }

    if (loaneeInput.value != "new") {
        return isValid;
    }

    let name = document.getElementById("name");
    if (name.value == "") {
        name.classList.add("is-invalid");
        isValid = 0;
    }

    return isValid;

}


async function createLoanee() {
    let  body = {
        "name": document.getElementById("name").value,
        "email": null,
        "phone": null,
        "note": null
      }

    if (document.getElementById("email").value != "") {
        body.email = document.getElementById("email").value;
    }

    if (document.getElementById("phone").value != "") {
        body.phone = document.getElementById("phone").value;
    }

    let sessionKey = localStorage.getItem("sessionKey");
      
      return fetch(`/api/loans/loanees?sessionKey=${sessionKey}`, {
          method: 'POST',
          body: JSON.stringify(body),
          headers: {
            'Content-Type': 'application/json'
          }
      })
      .then(response => {
          return response.json()
      })
      .then(data => {
        return data;
      })
      .catch(error => console.error(error));
}
