//this script is responsible for checking to see if an admin user exists when the client tries to access the register page.
//if an admin user exists (indicated by a 401 from the back-end), then the client is redirected to a login page. 

checkForAdminUser()

function checkForAdminUser() {
    fetch('http://localhost:5000/api/register/canRegister', {
        method: 'GET'
    })
    .then(response => {
        if (response.status === 401) {
            window.location.href = "login.html";
        }
        
    })
    .then(data => console.log(data))
    .catch(error => console.error(error));
}