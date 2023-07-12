//this file is responsible for handling log-in requests and session key storage on the client side. 


function submitLoginRequest() {
    event.preventDefault()
    let usernameInput = document.getElementById("usernameLoginInput");
    let passwordInput = document.getElementById("passwordLoginInput");
    let username = usernameInput.value;
    let password = passwordInput.value;
    let payload = {
        "Username": username,
        "Password": password
    }
    fetch('/api/login', {
        method: 'POST',
        body: JSON.stringify(payload),
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => {
            if (response.status === 401) {
                throw new Error("Incorrect Username/Password");
            }
            return response.json()
        })
        .then(data => storeSessionKey(data))
        .catch(error => {
            informIncorrectPassword();
            console.error(error)
        });
}

//if login is successful, store the session key in localstorage for the rest of the site to use. 
function storeSessionKey(key) {
    localStorage.setItem("sessionKey", key);
    setTimeout(function () {
        window.location.href = "index.html";
    }, 5);
}

function informIncorrectPassword() {
    loginInfo = document.getElementById("loginInfo");
    loginInfo.innerText = "Incorrect Username/Password";
    loginInfo.style.color = "red";

}


function submitRegisterRequest() {
    let nameInput = document.getElementById("nameRegisterInput");
    let emailInput = document.getElementById("emailRegisterInput");
    let usernameInput = document.getElementById("usernameRegisterInput");
    let passwordInput = document.getElementById("passwordRegisterInput");
    let passwordInput2 = document.getElementById("passwordRegisterInput2");
    if (passwordInput.value != passwordInput2.value) {
        informPasswordsDontMatch();
        return;
    }
    let name = nameInput.value;
    if (name == "") {
        name = "";
    }
    let email = emailInput.value;
    if (email == "") {
        email = "";
    }
    let username = usernameInput.value;
    let password = passwordInput.value;
    if (username == "" || password == "") {
        informNecessaryValues()
        return;
    }
    let payload = {
        "name": name,
        "email": email,
        "username": username,
        "password": password,
        "isAdmin": "1"
    }
    fetch('/api/register', {
        method: 'POST',
        body: JSON.stringify(payload),
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => {
            if (response.status === 401) {

            }
        })
        .then(data => redirectToLogin(data))
        .catch(error => console.error(error));
}

function informNecessaryValues() {
    registerInfo = document.getElementById("registerTitle");
    registerInfo.innerText = "Username & Password are required for registration.";
    registerInfo.style.color = "red";
    document.getElementById("usernameRegisterInput").classList.add("is-invalid");
    document.getElementById("passwordRegisterInput").classList.add("is-invalid");
    document.getElementById("passwordRegisterInput2").classList.add("is-invalid");
}

function informPasswordsDontMatch() {
    document.getElementById("passwordRegisterInput").classList.add("is-invalid");
    document.getElementById("passwordRegisterInput2").classList.add("is-invalid");
    registerInfo = document.getElementById("registerTitle");
    registerInfo.innerText = "Passwords must match.";
    registerInfo.style.color = "red";
}

function redirectToLogin(data) {
    window.location.href = "login.html";
}