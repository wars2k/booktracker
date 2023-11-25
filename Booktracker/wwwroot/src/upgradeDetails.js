handleUpgradeDetails();

function getUpgradeIDFromURL() {
    let urlParams = new URLSearchParams(window.location.search);
    upgradeID = urlParams.get('id');
    
    return upgradeID
}

async function handleUpgradeDetails() {
    let id = getUpgradeIDFromURL();
    let data = await getUpgradeData(id);
    displayData(data);
    
}

async function getUpgradeData(id) {
    let sessionKey = localStorage.getItem("sessionKey");
      
        return fetch(`/api/settings/upgrades/${id}?sessionKey=${sessionKey}`, {
          method: 'GET',
      })
      .then(response => {
          if (response.status === 401) {
              console.log("error");
          }
          //console.log("TEST");
          return response.json()
      })
      .then(data => {
        return data;
      })
      .catch(error => console.error(error));
}

function displayData(data) {
    document.getElementById("upgradeTitle").innerText = data.scriptInfo.title;
    document.getElementById("upgradeVersion").innerText = data.scriptInfo.version;
    document.getElementById("backupSize").innerText = data.backupSize;
    document.getElementById("backupPath").innerText = data.scriptInfo.backupPath
    document.getElementById("upgradeDescription").innerText = data.scriptInfo.description;
    document.getElementById("logs").innerText = data.logText;
    document.getElementById("logPath").innerText = data.scriptInfo.logPath;
}