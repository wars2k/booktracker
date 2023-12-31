init();

let filters = {
    "bookListID": null,
    "status": null,
    "loaneeID": null
}

async function init() {
    await buildLoaneeList();
    await buildTitleList();
    await buildLoanTable();
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
    let list = document.getElementById("loaneeList");

    let element = document.createElement("a");
    element.classList.add("dropdown-item");
    element.href = "#"
    element.innerText = loanee.name;
    element.addEventListener("click", function() { handleClick("loanee", loanee)});
    list.append(element);
}

async function buildTitleList() {
    let books = await getTitles();
    for (let book of books) {
        addBookToList(book);
    }
}

function addBookToList(book) {
    let list = document.getElementById("titleList");

    let element = document.createElement("a");
    element.classList.add("dropdown-item");
    element.href = "#"
    element.innerText = book.title;
    element.addEventListener("click", function() { handleClick("title", book)});
    list.append(element); 
}

async function getTitles() {
    let sessionKey = localStorage.getItem("sessionKey");
    let body = {
        "sessionKey": sessionKey
    }
    
      
      return fetch(`/api/getBookList`, {
          method: 'PUT',
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

function handleClick(type, value, subvalue) {
    if (type == "loanee") {
        document.getElementById("loaneeButton").innerText = value.name;
        filters.loaneeID = value.id;
    } else if (type == "status") {
        document.getElementById("statusButton").innerHTML = value;
        filters.status = subvalue;
    } else if (type == "title") {
        document.getElementById("titleButton").innerText = value.title;
        filters.bookListID = value.id;
    }

    buildLoanTable();
}

async function buildLoanTable() {
    let tableBody = document.getElementById("loanTableBody");
    tableBody.innerHTML = "";

    let loans = await getLoans();
    

    for (let loan of loans) {
        
        addLineToLoanTable(loan);
    }
}

async function getLoans() {
    let sessionKey = localStorage.getItem("sessionKey");
    let url = `/api/loans/?sessionKey=${sessionKey}`;
    
    if (filters.bookListID != null) {
        url += `&bookListID=${filters.bookListID}`
    }
    if (filters.status != null) {
        url += `&status=${filters.status}`
    }
    if (filters.loaneeID != null) {
        url += `&loaneeID=${filters.loaneeID}`
    }
    
      return fetch(url, {
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

function addLineToLoanTable(loan) {
    let tableBody = document.getElementById("loanTableBody");

    let row = document.createElement("tr");

    let status = document.createElement("td");
    status.append(getStatusBadge(loan.status, loan.id));
    row.append(status);

    let title = document.createElement("td");
    title.innerHTML = `<a href="bookPage.html?bookListId=${loan.bookListID}">${loan.bookTitle}</a>`
    row.append(title);

    let loanee = document.createElement("td");
    loanee.innerText = loan.loaneeName;
    row.append(loanee);

    let loanDate = document.createElement("td");
    loanDate.append(getDateInput(loan.loanDate, loan.id, "loanDate"))
    row.append(loanDate);

    let returnDate = document.createElement("td");
    returnDate.append(getDateInput(loan.returnDate, loan.id, "returnDate"))
    row.append(returnDate);

    let info = document.createElement("td");
    let infoButton = document.createElement("button");
    infoButton.classList.add("btn", "btn-sm");
    infoButton.innerText = "Info";
    infoButton.setAttribute("data-bs-toggle", "modal");
    infoButton.setAttribute("data-bs-target", "#loanInfoModal");
    infoButton.addEventListener("click", function() {
        openModal(loan.comment, loan.id);
    });
    info.append(infoButton);
    row.append(info);

    tableBody.append(row);
}

function getStatusBadge(status, id) {
    let div = document.createElement("div");

    let span = document.createElement("span");
    span.classList.add("badge", "dropdown-toggle");
    span.innerText = status;
    span.setAttribute('data-bs-toggle', 'dropdown');
    
    div.append(span);

    if (status == "LOANED") {
        span.classList.add("bg-blue-lt");
    }

    if (status == "RETURNED") {
        span.classList.add("bg-green-lt");
    }

    if (status == "NOT RETURNED") {
        span.classList.add("bg-red-lt");
    }

    let menu = document.createElement("div");
    menu.classList.add("dropdown-menu");
    menu.style.zIndex = 100000
    menu.append(getMenuItem("LOANED", "blue", id));
    menu.append(getMenuItem("RETURNED", "green", id));
    menu.append(getMenuItem("NOT RETURNED", "red", id));

    div.append(menu);

    return div;
}

function getMenuItem(status, color, id) {
    let link = document.createElement("a");
    link.href = "#";
    link.classList.add("dropdown-item");
    link.addEventListener("click", function() { updateLoan("status", id, status)})
    let span = document.createElement("span");
    span.classList.add("badge", `bg-${color}-lt`);
    span.innerText = status;

    link.append(span);

    return link;
}

async function updateLoan(type, id, value) {
    let body = {
        "status": null,
        "loanDate": null,
        "returnDate": null,
        "comment": null
    }

    switch (type) {
        case "status":
            body.status = value;
            break;
        case "loanDate":
            body.loanDate = value.value;
            break;
        case "returnDate":
            body.returnDate = value.value;
            break;
        case "comment":
            body.comment = document.getElementById("loanComment").value;
            break;
        default:
            break;
    }
    await submitUpdates(body, id);
    //await buildLoanTable();

}

async function submitUpdates(body, id) {
    let sessionKey = localStorage.getItem("sessionKey");
    fetch(`/api/loans/${id}?sessionkey=${sessionKey}`, {
        method: 'PUT',
        body: JSON.stringify(body),
        headers: {
          'Content-Type': 'application/json'
        }
    })
    .then(response => {
        buildLoanTable();
    })
    
    .catch(error => console.error(error));
}

function getDateInput(date, id, type) {
    let input = document.createElement("input");
    input.type = "date";
    input.value = date;
    input.classList.add("form-control");
    input.classList.add("date-input");

    input.onchange = function() {updateLoan(type, id, input)}
    return input;
}

function openModal(comment, id) {
    
    if (comment != undefined) {
        document.getElementById("loanComment").value = comment;
    } else {
        document.getElementById("loanComment").value = "";
    }
    document.getElementById("loanComment").style.height = "150px";
    const button = document.getElementById("updateCommentButton");
    const clone = button.cloneNode(true);
    button.parentNode.replaceChild(clone, button);
    clone.addEventListener("click", function() {
        updateLoan("comment", id, null)
    })
}

function insertTimestamp() {
    let date = new Date;
    let timestamp = `${date.getMonth() + 1}/${date.getDate()}/${date.getFullYear()} ${date.getHours()}:${date.getMinutes()}`;
    if (document.getElementById("loanComment").value == "") {
        document.getElementById("loanComment").value = timestamp + " - ";
        return;
    }
    document.getElementById("loanComment").value += `\r\n\r\n${timestamp} -  `
}
