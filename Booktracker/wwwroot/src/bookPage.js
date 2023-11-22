let globalBookListID = null;
let globalPageCount;
let globalStatus;
let globalFinishedDate;
let globalRating;

setUpBookPage()

document.getElementById('editButton').href = `editBook.html?bookListID=${globalBookListID}`

//gets the bookList ID located in the URL that is passed from the bookList table.
function getBookIDfromURL() {
    let urlParams = new URLSearchParams(window.location.search);
    bookListID = urlParams.get('bookListId');
    globalBookListID = bookListID;
    return bookListID
}

async function setUpBookPage() {
    let id = getBookIDfromURL();
    let data = await getBookData(id);
    globalPageCount = data.pageCount;
    globalStatus = data.status;
    globalFinishedDate = data.dateFinished;
    globalRating = data.rating;
    fillBookData(data);
    fillBookMetaData(data);
    console.log(data);
}

function getBookData(id) {
    let sessionKey = localStorage.getItem("sessionKey");
      
      return fetch(`/api/Booklist/${id}/data?sessionKey=${sessionKey}`, {
          method: 'GET',
      })
      .then(response => {
          if (response.status === 401) {
              informIncorrectPassword()
          }
          console.log("TEST");
          return response.json()
      })
      .then(data => {
        console.log("TEST2")
        return data;
      })
      .catch(error => console.error(error));
}

function fillBookData(data) {

    let title = document.getElementById("title");
    title.innerHTML = "<i>" + data.title + "<i>";

    let author = document.getElementById("author");
    author.innerText = data.author;

    let publisher = document.getElementById("publisher");
    publisher.innerText = data.publishedDate;

    let category = document.getElementById("category");
    category.innerText = data.category;

    let pageCount = document.getElementById("pageCount");
    pageCount.innerText = data.pageCount + " pages";

    let isbn = document.getElementById("isbn");
    isbn.innerText = data.isbn;

    let description = document.getElementById("description");
    description.innerHTML = data.description;

    let coverImage = document.getElementById("coverImage");
    coverImage.src = data.imageLink;
    

}

function fillBookMetaData(data) {

    let startDate = document.getElementById("startDate");
    startDate.value = data.dateStarted;

    let finishDate = document.getElementById("finishDate");
    finishDate.value = data.dateFinished;

    let owner = document.getElementById("owner");
    owner.innerText = data.username;

    let bookListID = document.getElementById("bookListID");
    bookListID.innerText = data.id;

    let rating = document.getElementById("rating");
        if (data.rating != null) {
            //rating.classList.add("status");
            switch (data.rating) {
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

        let status = document.getElementById("status");
        if (data.status != null) {
            status.innerText = data.status;
            status.classList.add("status");
            status.classList.add("statusChanges");
            
            switch (data.status) {
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
              case "DNF":
                status.classList.add("status-red");
                break;
              case "TO READ":
                status.classList.add("status-azure");
                break;
            }
        }
}

function toggleModal() {
  if (document.getElementById("deleteBookModal").style.display == "none") {
      document.getElementById("deleteBookModal").style.display = "block";
  } else {
      document.getElementById("deleteBookModal").style.display = "none"
  }
}

function submitBookDelete() {
  sessionKey = localStorage.getItem("sessionKey");
  let id = getBookIDfromURL()
  let payload = {
    "sessionKey": sessionKey
  }

  fetch(`/api/BookList/${id}/delete`, {
    method: 'DELETE',
    body: JSON.stringify(payload),
    headers: {
        'Content-Type': 'application/json'
    }
  })
  .then(response => response.json())
  .then(data => redirectToBookList())
  .catch(error => redirectToBookList());
}

function redirectToBookList() {
  window.location = "booklist.html"
}

function submitUpdate(payload) {
  let bookListID = getBookIDfromURL()
  fetch(`/api/BookList/${bookListID}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
    headers: {
      'Content-Type': 'application/json'
  }
  })
  .then(response => response.json())
  .then(data => refreshPage(data))
  .catch(error => refreshPage(error));
}

class UpdateBody {
  sessionKey;
  data;

  constructor(id) {

    this.sessionKey = localStorage.getItem("sessionKey");
    this.data = {
      "id": id,
      "rating": null,
      "status": null,
      "startDate": null,
      "finishedDate": null
    }

  }

}

function updateRating(rating) {
  if (rating == globalRating) {
    return;
  }
  let payload = new UpdateBody(getBookIDfromURL());
  payload.data.rating = rating;
  submitUpdate(payload)
}

function updateStatus(status) {
  if (status == globalStatus) {
    return;
  }
  let payload = new UpdateBody(getBookIDfromURL());
  payload.data.status = status;
  submitUpdate(payload)
}

function refreshPage() {
  location.reload();
}

function updateDate(type) {
  let payload = new UpdateBody(getBookIDfromURL());

  if (type == "start") {
    payload.data.startDate = document.getElementById("startDate").value;
  } else {
    payload.data.finishedDate = document.getElementById("finishDate").value;
  }

  submitUpdate(payload);
}
