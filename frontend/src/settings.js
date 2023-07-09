function getExportData(format) {
    let sessionKey = localStorage.getItem("sessionKey");
    fetch(`http://localhost:5000/api/data/export?format=${format}&sessionKey=${sessionKey}`)
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
    fetch(`http://localhost:5000/api/data/import?format=${format}&sessionKey=${sessionKey}`, {
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