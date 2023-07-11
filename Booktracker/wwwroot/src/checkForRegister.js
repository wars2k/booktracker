//this file automatically redirects from login.html to register.html if there is no admin user.

checkForAdminUser()

function checkForAdminUser() {
    fetch('/api/register/canRegister', {
        method: 'GET'
    })
    .then(response => {
        if (response.status === 401) {
            return;
        }
        window.location.href = "register.html";
        return
    })
    .then(data => console.log(data))
    .catch(error => console.error(error));
}
