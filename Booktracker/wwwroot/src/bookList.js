let bookData = {}
getAllBooks();

function createBookCards() {
  hideBookCards();
  document.getElementById("tableContainer").style.display = "none";
  document.getElementById("row").style.height = "auto";
  for (let i = 0; i < bookData.length; i++) {
    let card = document.createElement("div");
    card.classList.add("col-2");
    card.classList.add("bookResponse");
    card.innerHTML = `<div class="card" onclick="openBookDataPage(${bookData[i].id})" onmouseover="this.style.cursor='pointer';"> \
                        <div class="card-body bookContainer"> \
                          <div class="imageContainer"> \
                            <img src=${bookData[i].imageLink}> \
                          </div> \
                        </div>\
                      </a>`
    document.getElementById("row").append(card); 
  } 
}

function hideBookCards() {
  let bookCards = document.getElementsByClassName("bookResponse");
  let bookCardsArray = Array.from(bookCards);
    for (let i = 0; i < bookCardsArray.length; i++) {
      bookCardsArray[i].style.display = "none"; 
    }
}


function createBookTable(bookList) {
    //console.log(bookList);
    if (bookList == null) {
      bookList = bookData
    }
    hideBookCards();
    bookData = bookList;
    document.getElementById("tableContainer").style.display = "";
    let table = document.getElementById("bookListTableBody");
    table.style.display = "";
    table.innerHTML = "";
    for (let i = 0; i < bookList.length; i++) {
      let row = document.createElement("tr");

        let title = document.createElement("td");
        title.innerText = bookList[i].title;
        row.append(title);

        let author = document.createElement("td");
        author.innerText = bookList[i].author;
        row.append(author);

        let publisher = document.createElement("td");
        if (bookList[i].publisher != null) {
          publisher.innerText = bookList[i].publisher;
        }
        row.append(publisher);

        let publishedDate = document.createElement("td");
        if (bookList[i].publishedDate != null) {
          publishedDate.innerText = bookList[i].publishedDate;
        }
        row.append(publishedDate);

        let statusBox = document.createElement("td");
        let status = document.createElement("div");
        
        if (bookList[i].status != null) {
            status.innerText = bookList[i].status;
            status.classList.add("status");
            status.classList.add("statusChanges");
            
            switch (bookList[i].status) {
              case "UNASSIGNED":
                status.classList.add("status-yellow")
                break;
              case "READING":
                status.classList.add("status-green")
                break;
              case "UP NEXT":
                status.classList.add("status-teal");
                break;
              case "TO READ":
                status.classList.add("status-azure");
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
            }
        }
        statusBox.append(status);
        row.append(statusBox);

        let ratingBox = document.createElement("td");
        let rating = document.createElement("div");
        if (bookList[i].rating != null) {
            rating.classList.add("status");
            switch (bookList[i].rating) {
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
        ratingBox.append(rating);
        row.append(ratingBox);

        let dateStarted = document.createElement("td");
        if (bookList[i].dateStarted != null) {
            dateStarted.innerText = bookList[i].dateStarted;
        }
        row.append(dateStarted);

        let dateFinished = document.createElement("td");
        if (bookList[i].dateFinished != null) {
            dateFinished.innerText = bookList[i].dateFinished;
        }
        row.append(dateFinished)

        let editButtonData = document.createElement("td");
        let editButton = document.createElement("button");
        editButtonData.append(editButton);
        editButton.innerText = "Edit";
        editButton.classList.add("btn");
        editButton.style.height = "20px";
        editButton.style.width = "30px"
        editButton.addEventListener("click", function() {
          editBookListEntry(bookList[i].id);
        })
        row.append(editButtonData);

        let detailButtonData = document.createElement("td");
        let detailButton = document.createElement("img");
        detailButtonData.append(detailButton);
        detailButton.src = "styles/list-details.png";
        detailButton.classList.add("icon");
        //detailButton.style.height = "25px";
        detailButton.style.width = "15px";
        detailButton.addEventListener("click", function() {
          openBookDataPage(bookList[i].id);
        })
        row.append(detailButtonData);

      table.append(row); 
    }
    if (localStorage.getItem("filter") != "null") {
      filterByStatus(true);
    }
  }
  //queries the database for a list of all books, then calls createBookTable() to display the queried data.
  //the back-end returns the data in an array of objects with the following properties:
  //   id, title, author, publisher, publishedDate
  function getAllBooks() {
    let payload = {
        "sessionKey": localStorage.getItem("sessionKey")
    }
    fetch('/api/getBookList', {
        method: 'PUT',
        body: JSON.stringify(payload),
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (response.status === 401) {
            informIncorrectPassword()
        }
        return response.json()
    })
    .then(data => createBookTable(data))
    .catch(error => console.error(error));
  }


  function editBookListEntry(id) {
    let submitButton = document.getElementById("submitEditButton");
    let clone = submitButton.cloneNode(true);
    submitButton.parentNode.replaceChild(clone, submitButton);
    let deleteButton = document.getElementById("deleteBookButton");
    let deleteClone = deleteButton.cloneNode(true);
    deleteButton.parentNode.replaceChild(deleteClone, deleteButton);
    let editBox = document.getElementById("editBox");
    editBox.style.display = "block";
    clone.addEventListener("click", function() {
      gatherEdits(id);
      //console.log("test");
      hideEditBox();
      getAllBooks();
      
    })
    deleteClone.addEventListener("click", function() {
      deleteBookFromList(id);
      hideEditBox();
      getAllBooks();
    })

    
  }

  function hideEditBox() {
    let editBox = document.getElementById("editBox");
    updateStatusView('NO CHANGES', 'status');
    updateRatingView({innerText: "NO CHANGES"}, 'status')
    document.getElementById("startDateEdit").value = "";
    document.getElementById("finishedDateEdit").value = "";
    editBox.style.display = "none";
  }

  function updateStatusView(status, classes) {
    let editStatusBox = document.getElementById("editStatusBox");
    editStatusBox.className = "";
    editStatusBox.classList.add("status");
    editStatusBox.classList.add(classes);
    editStatusBox.innerText = status;
    if (status == "FINISHED") {
      let finishedDate = document.getElementById("finishedDateEdit");
      finishedDate.value = new Date().toLocaleDateString('sv');
    }
  }

  function updateRatingView(content, classes) {
    let editRatingBox = document.getElementById("editRatingBox");
    editRatingBox.className = "";
    editRatingBox.classList.add("status");
    editRatingBox.classList.add(classes);
    editRatingBox.innerText = content.innerText;

  }

  function gatherEdits(id) {
    let editData = {}
 
    let editStatusBox = document.getElementById("editStatusBox");
    let editRatingBox = document.getElementById("editRatingBox");
    let startDate = document.getElementById("startDateEdit");
    let finishedDate = document.getElementById("finishedDateEdit");
    if (editStatusBox.innerText != "NO CHANGES") {
      editData.status = editStatusBox.innerText;
    } else {
      editData.status = null;
    }
    if (editRatingBox.innerText != "NO CHANGES") {
      editData.rating = getStarCount(editRatingBox.innerHTML);
    } else {
      editData.rating = null;
    }
    if (startDate.value == "") {
      editData.startDate = null;
    } else {
      editData.startDate = startDate.value;
    }
    if (finishedDate.value == "") {
      editData.finishedDate = null;
    } else {
      editData.finishedDate = finishedDate.value;
    }
    editData.id = id;

    submitEditData(editData);
  }

  function getStarCount(stars) {
    return stars.length;
  }

  function submitEditData(editData) {
    let payload = {
        "sessionKey": localStorage.getItem("sessionKey"),
        "data": editData
    }
    console.log(payload);
    fetch(`/api/BookList/${editData.id}`, {
        method: 'PUT',
        body: JSON.stringify(payload),
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (response.status === 401) {
            informIncorrectPassword()
        }
        //console.log("TEST");
        setTimeout(getAllBooks(), 150);
        return //response.json()
    })
    .then(data => {
      //console.log(data);
      
    })
    .catch(error => console.error(error));
  }

  function filterByStatus(justLoaded) {
    let statusFilterArray;
    let ratingFilterArray;
    document.getElementById("dateFilterButton").classList.add("disabled");

    if (justLoaded) {
      let filters = JSON.parse(localStorage.getItem("filter"));
      statusFilterArray = filters.status;
      ratingFilterArray = filters.rating;
      checkActiveFilters(statusFilterArray, ratingFilterArray);
    } else {
      statusFilterArray = getStatusFilterArray();
      ratingFilterArray = getRatingFilterArray();

      let filters = {status: statusFilterArray, rating: ratingFilterArray}
      localStorage.setItem("filter", JSON.stringify(filters));
    }

    let statusFilterLength = statusFilterArray.length;
    let ratingFilterLength = ratingFilterArray.length;

    if (statusFilterArray.length == 0 && ratingFilterArray.length == 0) {
      removeFilter();
      return;
    }

    document.getElementById("statusFilterButton").innerText = `Status (${statusFilterArray.length})`;
    document.getElementById("ratingFilterButton").innerText = `Rating (${ratingFilterArray.length})`;

    let tableBody = document.getElementById("bookListTableBody");

    for (const child of tableBody.children) {

      //just filter based on status
      if (ratingFilterLength == 0) {
        if (!statusFilterArray.includes(child.children[4].innerText)) {
          child.style.display = "none"
        } else {
          child.style.display = "";
        }
        continue;

      //or, just filter based on rating
      } else if (statusFilterLength == 0) {
        if (!ratingFilterArray.includes(child.children[5].innerText)) {
          child.style.display = "none"
        } else {
          child.style.display = "";
        }
        continue;
      }

      //or, filter based on both
      if (statusFilterArray.includes(child.children[4].innerText) && ratingFilterArray.includes(child.children[5].innerText)) {
        child.style.display = "";
      } else {
        child.style.display = "none";
      }

    }
  }

  function getStatusFilterArray() {
    let filterArray = [];
    for (const child of document.getElementById("statusFilter").children) {
      if (child.children[0].checked == 1) {
        filterArray.push(child.children[0].value);
      }
    }
    return filterArray;
  }

  function getRatingFilterArray() {
    let filterArray = [];
    for (const child of document.getElementById("ratingFilter").children) {
      if (child.children[0].checked == 1) {
        filterArray.push(child.children[0].value);
      }
    }
    return filterArray;
  }

  function removeFilter() {
    document.getElementById("statusFilterButton").innerText = `Status`;
    document.getElementById("ratingFilterButton").innerText = `Rating`;
    document.getElementById("dateFilterButton").classList.remove("disabled");
    localStorage.setItem("filter", null);
    let tableBody = document.getElementById("bookListTableBody");
    for (const child of tableBody.children) {
      child.style.display = "";
    }
  }

  function checkActiveFilters(statusFilterArray, ratingFilterArray) {

    //works for now, but probably not as efficient as it could be if there are more checkboxes eventually
    const checkboxElements = document.querySelectorAll("input[type='checkbox']");
    
    for (const checkbox of checkboxElements) {
      if (statusFilterArray.includes(checkbox.value) || ratingFilterArray.includes(checkbox.value)) {
        checkbox.checked = 1;
      }
    }
    
  }

  function searchTable() {
    
    var input = document.getElementById("bookListSearch");
    var table = document.getElementById("bookListTableBody");
  
    
    var rows = table.getElementsByTagName("tr");
  
    
    for (var i = 0; i < rows.length; i++) {
      var row = rows[i];
      var cells = row.getElementsByTagName("td");
      var showRow = false;
  
      
      for (var j = 0; j < cells.length; j++) {
        var cell = cells[j];
        if (cell.innerHTML.toLowerCase().indexOf(input.value.toLowerCase()) > -1) {
          showRow = true;
          break;
        }
      }

      if (showRow) {
        row.style.display = "";
      } else {
        row.style.display = "none";
      }
    }
  }

  function deleteBookFromList(id) {
    let payload = {
      "sessionKey": localStorage.getItem("sessionKey")
    }
    fetch(`/api/BookList/${id}/delete`, {
        method: 'DELETE',
        body: JSON.stringify(payload),
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (response.status === 401) {
            informIncorrectPassword()
        }
        //console.log("TEST");
        setTimeout(getAllBooks(), 150);
        return response.json()
    })
    .catch(error => console.error(error));
  }

  function openBookDataPage(id) {
    let url = "bookPage.html?bookListId=" + id;
    window.location.href = url;
  }

  function openDateFilter() {
    let button = document.getElementById("dateFilterButton");
    let content = document.getElementById("dateFilterContent");
    let classes = Array.from(content.classList);
    
     if (classes.includes("hidden")) {
       content.classList.remove("hidden");
     } else {
       content.classList.add("hidden");
       return;
     }

    const buttonRect = button.getBoundingClientRect();
    const buttonX = buttonRect.left;
    const buttonY = buttonRect.bottom - 10;
    

    content.style.left = buttonX + "px";
    content.style.top = buttonY + "px";

    
  }

  function updateDateFilter1(filterName) {
    document.getElementById("dateFilter1").innerText = filterName;
  }

  function updateDateFilter2(filterName) {
    document.getElementById("dateFilter2").innerText = filterName;
    let extraFilters = document.getElementById("extraDateFilters");
    if (filterName == "Between") {
      extraFilters.classList.remove("hidden");
    } else {
      extraFilters.classList.add("hidden");
    }
  }

  function filterByDate() {
    let isInputValid = validateFilterByDateInputs();
    if (!isInputValid) {
      return;
    }

    disableOtherFilters();

    let dateToUse = document.getElementById("dateFilter1").innerText;
    let columnIndex;

    if (dateToUse == "Started") {
      columnIndex = 6;
    } else {
      columnIndex = 7;
    }

    let filterType = document.getElementById("dateFilter2").innerText;
    let firstDate = document.getElementById("firstDate").value;
    let secondDate = document.getElementById("secondDate").value;

    if (filterType == "Between") {
      filterBetweenTwoDates(firstDate, secondDate, columnIndex);
      return;
    } else if (filterType == "On") {
      filterOnDate(firstDate, columnIndex);
      return;
    } else if (filterType == "After") {
      filterAfterDate(firstDate, columnIndex);
    } else if (filterType == "Before") {
      filterBeforeDate(firstDate, columnIndex);
    }
    openDateFilter();
  }

function disableOtherFilters() {
  document.getElementById("statusFilterButton").classList.add("disabled");
  document.getElementById("ratingFilterButton").classList.add("disabled");
}

function validateFilterByDateInputs() {

  let dateOne = document.getElementById("firstDate");
  let typeOfFilter = document.getElementById("dateFilter2");
  let dateTwo = document.getElementById("secondDate");

  dateOne.classList.remove("is-invalid");
  dateTwo.classList.remove("is-invalid");

  if (dateOne.value == "") {
    dateOne.classList.add("is-invalid");
    return false;
  }

  if (typeOfFilter.innerText == "Between") {

    if (dateTwo.value == "") {
      dateTwo.classList.add("is-invalid");
      return false;
    }

    let dateOneRealDate = new Date(dateOne.value);
    let dateTwoRealDate = new Date(dateTwo.value);

    if (dateOneRealDate > dateTwoRealDate) {
      dateOne.classList.add("is-invalid");
      dateTwo.classList.add("is-invalid");
      return false;
    } 

  }

  return true;

}

function filterAfterDate(dateString, columnIndex) {

  let date = new Date(dateString);
  let tableBody = document.getElementById("bookListTableBody");
  removeFilter();

  for (const child of tableBody.children) {

    let dateToCompare = child.children[columnIndex].innerText;
    if (dateToCompare == "") {
      child.style.display = "none";
      continue;
    }
    dateToCompare = new Date(dateToCompare);

    if (date >= dateToCompare) {
      child.style.display = "none";
    }

  }
}

function filterBeforeDate(dateString, columnIndex) {

  let date = new Date(dateString);
  let tableBody = document.getElementById("bookListTableBody");
  removeFilter();

  for (const child of tableBody.children) {

    let dateToCompare = child.children[columnIndex].innerText;
    if (dateToCompare == "") {
      child.style.display = "none";
      continue;
    }
    dateToCompare = new Date(dateToCompare);

    if (date <= dateToCompare) {
      child.style.display = "none";
    }

  }
}

function filterOnDate(dateString, columnIndex) {

  let date = new Date(dateString);
  let tableBody = document.getElementById("bookListTableBody");
  removeFilter();

  for (const child of tableBody.children) {

    let dateToCompare = child.children[columnIndex].innerText;
    if (dateToCompare == "") {
      child.style.display = "none";
      continue;
    }
    dateToCompare = new Date(dateToCompare);

    if (date.getTime() != dateToCompare.getTime()) {
      child.style.display = "none";
    }

  }
}

function filterBetweenTwoDates(firstDate, secondDate, columnIndex) {

  firstDate = new Date(firstDate);
  secondDate = new Date(secondDate);
  let tableBody = document.getElementById("bookListTableBody");
  removeFilter();

  for (const child of tableBody.children) {

    let dateToCompare = child.children[columnIndex].innerText;
    if (dateToCompare == "") {
      child.style.display = "none";
      continue;
    }
    dateToCompare = new Date(dateToCompare);

    if (!(dateToCompare >= firstDate && dateToCompare <= secondDate)) {
      child.style.display = "none";
    }

  }

}

function clearDateFilter() {
  removeFilter();
  document.getElementById("statusFilterButton").classList.remove("disabled");
  document.getElementById("ratingFilterButton").classList.remove("disabled");
}

  
