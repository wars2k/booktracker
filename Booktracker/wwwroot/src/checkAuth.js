//this script is responsible for checking to see if the user is logged in. If there is no sessionKey, then the user is automatically pushed to the login page. 

if (localStorage.getItem("sessionKey") == "undefined" || localStorage.getItem("sessionKey") == "null" || localStorage.getItem("sessionKey") == null) {
    window.location.href = "login.html";
}

function logOut() {
    fetch('/api/logout', {
        method: 'POST',
    })
        .then(response => {
            return response.json()
        })
        .then(data => console.log(data))
        .catch(error => console.error(error));
    localStorage.removeItem("sessionKey");
    window.location.href = "login.html";
}