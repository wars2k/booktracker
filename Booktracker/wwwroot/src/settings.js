function getExportData(format) {
    let sessionKey = localStorage.getItem("sessionKey");
    fetch(`/api/data/export?format=${format}&sessionKey=${sessionKey}`)
  .then(response => response.blob())
  .then(blob => {
    // Create a temporary URL for the blob object
    const url = URL.createObjectURL(blob);

    // Create a link element
    const link = document.createElement('a');
    link.href = url;
    link.download = `export.${format}`;

    // Append the link to the document body
    document.body.appendChild(link);

    // Programmatically click the link to trigger the file download
    link.click();

    // Clean up the temporary URL and remove the link element
    URL.revokeObjectURL(url);
    link.remove();
  })
  .catch(error => {
    // Handle any errors
    console.error('Error downloading file:', error);
  });
}

function submitDataImport() {
    let format = document.getElementById("importFormat").value;
    let sessionKey = localStorage.getItem("sessionKey");
    let fileInput = document.getElementById("fileInput");
    let file = fileInput.files[0];
    let formData = new FormData();
    formData.append('file', file);
    fetch(`/api/data/import?format=${format}&sessionKey=${sessionKey}`, {
        method: 'POST',
        body: formData
      })
        .then(response => {
          // Handle the response
          if (response.ok) {
            alert("Import completed successfully.");
            console.log('File uploaded successfully');
          } else {
            // File upload failed
            alert("Import failed");
            console.error('File upload failed');
          }
        })
        .catch(error => {
          // Handle any errors
          alert("Import failed");
          console.error('Error uploading file:', error);
        });
}
getMostRecentVersion();
function getMostRecentVersion() {
  fetch(`https://api.github.com/repos/wars2k/booktracker/releases`, {
    method: 'GET',
    })
    .then(response => response.json())
    .then(data => displayMostRecentVersion(data))
    .catch(error => console.error(error));
}

function displayMostRecentVersion(data) {
  let version = document.getElementById("mostRecentVersion");
  version.innerText = data[0].tag_name;
}

function displayErrorGettingVersion(error) {
  let version = document.getElementById("mostRecentVersion");
  version.innerText = "Error getting latest version from Github.";
  console.error(error);
}

function getCurrentLoggingLevel() {
  let sessionKey = localStorage.getItem("sessionKey");
  fetch(`/api/settings/loggingLevel?sessionKey=${sessionKey}`, {
    method: 'GET',
    })
    .then(response => response.json())
    .then(data => displayLoggingLevel(data))
    .catch(error => console.error(error));
}

function displayLoggingLevel(data) {
  document.getElementById("loggingLevel").value = data;
}

getCurrentLoggingLevel();

function updateLoggingLevel() {
  let sessionKey = localStorage.getItem("sessionKey");
  let level = document.getElementById("loggingLevel").value;
  fetch(`/api/settings/loggingLevel?level=${level}&sessionKey=${sessionKey}`, {
    method: 'PUT',
  })
  .then(response => response.json())
    .then(data => console.log(data))
    .catch(error => console.error(error));
}


class UpgradeRow {
  id;
  version;
  title;
  dateTime;
  backupStatus = false;
  row;

  constructor(response) {
    this.id = response.id;
    this.version = response.version;
    this.title = response.title;
    this.dateTime = this.formatDateTime(response.completedDateTime);

    if (response.backupPath != null) {
      this.backupStatus = true;
    }

    this.buildRow()

  }

  formatDateTime(dateTime) {

    //dateTime = dateTime.replace(/-/g, '/');
    dateTime = dateTime.split(".")

    return dateTime[0]

  }

  buildRow() {

    let row = document.createElement("tr");
    row.style.fontSize = "11pt"

    let id = document.createElement("td");
    id.innerText = this.id;
    row.append(id);

    let version = document.createElement("td");
    let versionContent = document.createElement("code");
    versionContent.innerText = this.version;
    version.append(versionContent);
    row.append(version);
    
    let title = document.createElement("td");
    title.innerHTML = `<a href="upgradeDetails.html?id=${this.id}">${this.title}</a>`
    row.append(title)

    let dateTime = document.createElement("td");
    dateTime.innerText = this.dateTime;
    row.append(dateTime);

    let backupData = document.createElement("td");
    let badge = document.createElement("span");
    badge.classList.add("badge", "bg-green-lt");
    badge.innerText = "Successful";
    if (this.backupStatus == true) {
      backupData.append(badge);
    }

    row.append(backupData);

    this.row = row;
    
  }
}

upgradeTableHandler();

async function upgradeTableHandler() {
  let rows = await getUpgradeData();
  if (rows.length == 0) {
    document.getElementById("tableWrapper").innerHTML = '<code>No upgrades scripts have ran.</code>'
    return;
  }
  for (let i = 0; i < rows.length; i++) {
    let rowObject = new UpgradeRow(rows[i]);
    document.getElementById("upgradeTableBody").append(rowObject.row);
  }
}

async function getUpgradeData() {
  let sessionKey = localStorage.getItem("sessionKey");
      
      return fetch(`/api/settings/upgrades?sessionKey=${sessionKey}`, {
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