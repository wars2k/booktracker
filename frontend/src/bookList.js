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
    console.log(bookList);
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
              case "WISHLIST":
                status.classList.add("status-blue");
                break;
              case "FINISHED":
                status.classList.add("status-purple");
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
        if (bookList[i].rating != null) {
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
        editButton.style.height = "25px";
        editButton.style.width = "50px"
        editButton.addEventListener("click", function() {
          editBookListEntry(bookList[i].id);
        })
        row.append(editButtonData);

        let detailButtonData = document.createElement("td");
        let detailButton = document.createElement("img");
        detailButtonData.append(detailButton);
        detailButton.src = "styles/list-details.svg";
        detailButton.classList.add("icon");
        //detailButton.style.height = "25px";
        detailButton.style.width = "20px";
        detailButton.addEventListener("click", function() {
          openBookDataPage(bookList[i].id);
        })
        row.append(detailButtonData);

      table.append(row); 
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
        header: {
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
      console.log("test");
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
    editBox.style.display = "none";
  }

  function updateStatusView(status, classes) {
    let editStatusBox = document.getElementById("editStatusBox");
    editStatusBox.className = "";
    editStatusBox.classList.add("status");
    editStatusBox.classList.add(classes);
    editStatusBox.innerText = status;
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
    
    let numberOfStars;
    switch (stars) {
      case "☆☆☆☆☆":
        numberOfStars = "5";
        break;
      case "☆☆☆☆":
        numberOfStars = "4";
        break;
      case "☆☆☆":
        numberOfStars = "3";
        break;
      case "☆☆":
        numberOfStars = "2";
        break;
      case "☆":
        numberOfStars = "1";
        break;
      default:
        numberOfStars = null;
        break;
    }
    
    return numberOfStars;
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
        header: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (response.status === 401) {
            informIncorrectPassword()
        }
        console.log("TEST");
        setTimeout(getAllBooks(), 150);
        return response.json()
    })
    .then(data => {
      console.log(data);
      
    })
    .catch(error => console.error(error));
  }

  function filterByStatus(status) {
    removeFilter();
    let tableBody = document.getElementById("bookListTableBody");
    for (const child of tableBody.children) {
      //console.log(child.children[4].innerText);
      if (child.children[4].innerText != status) {
        child.style.display = "none";
      }
    }
  }

  function removeFilter() {
    let tableBody = document.getElementById("bookListTableBody");
    for (const child of tableBody.children) {
      child.style.display = "";
    }
  }

  function searchTable() {
    // Get input element and table element
    var input = document.getElementById("bookListSearch");
    var table = document.getElementById("bookListTableBody");
  
    // Get all rows in the table
    var rows = table.getElementsByTagName("tr");
  
    // Loop through all rows and hide those that do not match the search query
    for (var i = 0; i < rows.length; i++) {
      var row = rows[i];
      var cells = row.getElementsByTagName("td");
      var showRow = false;
  
      // Loop through all cells in the row and check for a match with the search query
      for (var j = 0; j < cells.length; j++) {
        var cell = cells[j];
        if (cell.innerHTML.toLowerCase().indexOf(input.value.toLowerCase()) > -1) {
          showRow = true;
          break;
        }
      }
  
      // Show or hide the row based on the search query
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
    fetch(`http://localhost:5000/api/BookList/${id}/delete`, {
        method: 'DELETE',
        body: JSON.stringify(payload),
        header: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (response.status === 401) {
            informIncorrectPassword()
        }
        console.log("TEST");
        setTimeout(getAllBooks(), 150);
        return response.json()
    })
    .catch(error => console.error(error));
  }

  function openBookDataPage(id) {
    let url = "bookPage.html?bookListId=" + id;
    window.location.href = url;
  }
  
