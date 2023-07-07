async function handleNewCollection() {
    let newCollectionData = {};
    newCollectionData.Name = document.getElementById("collectionName").value;
    newCollectionData.Description = document.getElementById("collectionDescription").value;
    newCollectionData.CoverImage = document.getElementById("collectionImage").value;
    console.log(newCollectionData);
    if (newCollectionData.Name == "") {
        document.getElementById("collectionName").classList.add("is-invalid");
        return;
    }
    if (newCollectionData.Description == "") {
        newCollectionData.Description = null;
    }
    if (newCollectionData.CoverImage == "") {
        newCollectionData.CoverImage = null;
    }
    console.log(newCollectionData)
    let status = await submitNewCollection(newCollectionData);
    if (status == 200) {
        createSuccessBanner();
    }
}

async function submitNewCollection(data) {
    try {
        const sessionKey = localStorage.getItem("sessionKey");
        const response = await fetch(`http://localhost:5000/api/collections/new?sessionKey=${sessionKey}`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify(data)
        });
        const statusCode = response.status;
        return statusCode;
      } catch (error) {
        console.error(error);
        throw error;
      }
}

function createSuccessBanner() {
    let banner = document.createElement("div");
    banner.classList.add("alert");
    banner.classList.add("alert-success");
    let title = document.createElement("h4");
    title.classList.add("alert-title");
    title.innerHTML = "Success!"
    banner.append(title);
    document.getElementById("page").prepend(banner);
}