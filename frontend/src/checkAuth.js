//this script is responsible for checking to see if the user is logged in. If there is no sessionKey, then the user is automatically pushed to the login page. 

if (localStorage.getItem("sessionKey") == "undefined" || localStorage.getItem("sessionKey") == "null" || localStorage.getItem("sessionKey") == null) {
    window.location.href = "login.html";
}
console.log(localStorage.getItem("sessionKey"))

function logOut() {
    fetch('http://localhost:5000/api/logout', {
        method: 'POST',
    })
    .then(response => {
        if (response.status === 200) {
            console.log("200");
        } else {
            console.log("FAILED");
        }
        return response.json()
    })
    .then(data => console.log(data))
    .catch(error => console.error(error));
    localStorage.removeItem("sessionKey");
    window.location.href = "login.html";
}