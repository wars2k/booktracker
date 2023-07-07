getAllUsers();

function getAllUsers() {
    let sessionKey = localStorage.getItem("sessionKey");
    fetch(`http://localhost:5000/api/users?sessionKey=${sessionKey}`, {
      method: 'GET'
    })
    .then(response => {
        if (response.status === 401) {
            informNotAuthorized()
        }
        return response.json()
    })
    .then(data => createUserTable(data))
    .catch(error => console.error(error));
}

function informNotAuthorized() {
    let body = document.getElementById("mainBody");
    let div = document.createElement("div");
    div.style.marginTop = "20px";
    div.innerHTML = "<code>ERROR: Admin privileges required to access user list</code>"
    body.append(div);
}

//creates a new row for each user. Also creates a button used for editing the users' data. 
function createUserTable(userList) {
    let table = document.getElementById("mainTableBody");
    table.innerHTML = "";
    for (let i = 0; i < userList.length; i++) {
      let row = document.createElement("tr");
        let id = document.createElement("td");
        id.innerText = userList[i].id;
        row.append(id);
        let name = document.createElement("td");
        name.innerText = userList[i].name;
        row.append(name);
        let username = document.createElement("td");
        username.innerText = userList[i].username;
        row.append(username);
        let email = document.createElement("td");
        email.innerText = userList[i].email;
        row.append(email);
        let isAdmin = document.createElement("td");
        if (userList[i].isAdmin == 0) {
            isAdmin.innerText = "FALSE";
        } else {
            isAdmin.innerText = "TRUE";
        }
        row.append(isAdmin);
        let editButtonData = document.createElement("td");
        let editButton = document.createElement("button");
        editButtonData.append(editButton);
        editButton.innerText = "Edit";
        editButton.classList.add("btn");
        editButton.style.height = "25px";
        editButton.style.width = "50px"
        editButton.addEventListener("click", function() {
          editUser(userList[i].id);
        })
        row.append(editButtonData);

      table.append(row); 
    }
  }

  function showNewUserBox() {
    let newUserBox = document.getElementById("newUserBox");
    newUserBox.style.display = "block";
  }

  function hideNewUserBox() {
    let newUserBox = document.getElementById("newUserBox");
    newUserBox.style.display = "none";
  }

  function submitNewUserData() {
    let data = gatherNewUserData();
    console.log("DATA: " + data);
    isValid = validateData(data);
    if (!isValid) {
        alertFailedSubmission()
        return;
    }
    pushData(data);
    hideNewUserBox();
    location.reload();
  }

  function gatherNewUserData() {
    let data = {};
    data.name = document.getElementById("newUserName").value;
    console.log(data.name)
    if (data.name == "") {
        data.name = null;
    }
    data.email = document.getElementById("newUserEmail").value;
    if (data.email == "") {
        data.email = null;
    }
    data.username = document.getElementById("newUserUsername").value;
    data.password = document.getElementById("newUserPassword").value;
    data.isAdmin = document.getElementById("newUserAdminPriv").value;
    console.log(data);
    return data;
  }

  function validateData(data) {
    if (data.username == "" || data.password == "") {
        return false
    }
    return true;
  }

  function alertFailedSubmission() {
    document.getElementById("newUserUsername").classList.add("is-invalid");
    document.getElementById("newUserPassword").classList.add("is-invalid");
  }

  function pushData(data) {
    sessionKey = localStorage.getItem("sessionKey");
    fetch(`http://localhost:5000/api/users/new?sessionKey=${sessionKey}`, {
      method: 'POST',
      body: JSON.stringify(data),
      headers: {
        'Content-Type': 'application/json'
      }
      })
      .then(response => response.json())
      .then(data => console.log(data))
      .catch(error => console.error(error));
  }

  function editUser(id) {
    let submitButton = document.getElementById("submitUserEdits");
    let clone = submitButton.cloneNode(true);
    submitButton.parentNode.replaceChild(clone, submitButton);
    let deleteButton = document.getElementById("deleteUser");
    let deleteClone = deleteButton.cloneNode(true);
    deleteButton.parentNode.replaceChild(deleteClone, deleteButton);
    let editUserBox = document.getElementById("editUserBox");
    editUserBox.style.display = "block";
    clone.addEventListener("click", function() {
      gatherEditUserData(id);
      hideEditUserBox();
      getAllUsers();
      location.reload();
      
    })
    deleteClone.addEventListener("click", function() {
      deleteUser(id);
      hideEditUserBox();
      getAllUsers();
      location.reload();
    })
  }

  function hideEditUserBox() {
    document.getElementById("editUserBox").style.display = "none";
  }

  function gatherEditUserData(id) {
    let data = {};
    data.name = document.getElementById("editUserName").value;
    if (data.name == "") {
        data.name = null;
    }
    data.email = document.getElementById("editUserEmail").value;
    if (data.email == "") {
        data.email = null;
    }
    data.username = document.getElementById("editUserUsername").value;
    if (data.username == "") {
        data.username = null;
    }
    data.isAdmin = document.getElementById("editUserAdminPriv").value;
    if (data.isAdmin == "-1") {
        data.isAdmin = null
    }
    pushEditData(id, data);
  }

  function pushEditData(id, data) {
    sessionKey = localStorage.getItem("sessionKey");
    fetch(`http://localhost:5000/api/users/${id}?sessionKey=${sessionKey}`, {
      method: 'PUT',
      body: JSON.stringify(data),
      headers: {
        'Content-Type': 'application/json'
      }
      })
      .then(response => response.json())
      .then(data => console.log(data))
      .catch(error => console.error(error));
  }

  function deleteUser(id) {
    let sessionKey = localStorage.getItem("sessionKey");
    fetch(`http://localhost:5000/api/users/${id}?sessionKey=${sessionKey}`, {
      method: 'DELETE'
    })
    .then(response => response.json())
    .then(data => console.log(data))
    .catch(error => console.error(error));
  }