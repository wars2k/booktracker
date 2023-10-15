challengeHandler();

/**
 * Gets all challenges for a given user.
 * @returns an array of challenges objects
 */
async function getChallenges() {
    let sessionKey = localStorage.getItem("sessionKey");
      
        return fetch(`/api/challenges?sessionKey=${sessionKey}`, {
          method: 'GET',
      })
      .then(response => {
          if (response.status === 401) {
              informIncorrectPassword()
          }
          //console.log("TEST");
          return response.json()
      })
      .then(data => {
        //console.log("TEST2")
        return data;
      })
      .catch(error => console.error(error));
}

/**
 * Gets all challenges, creates new objects for each of them,
 * and adds each as a card on the main page.
 */
async function challengeHandler() {
    let challenges = await getChallenges();
    console.log(challenges);
    if (challenges.length == 0) {
        return;
    }
    document.getElementById("container").innerHTML = "";
    
    for (let i = 0; i < challenges.length; i++) {
        const challenge = challenges[i];
        let newChallenge;
        if (challenge.type == "reading") {
            newChallenge = new ReadingChallenge(challenge);
        } else if (challenge.type == "writing") {
            newChallenge = new WritingChallenge(challenge);
        }
        document.getElementById("container").prepend(newChallenge.card);
        
    }
}

/**
 * Deletes a given challenge by ID.
 * @param {number} id The challenge ID that should be deleted.
 * @returns The response from the server after attempting to delete the challenge.
 */
async function deleteEntryAPI(id) {
    let sessionKey = localStorage.getItem("sessionKey");
      
        return fetch(`/api/challenges/${id}?sessionKey=${sessionKey}`, {
          method: 'DELETE',
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

/**
 * Calls the function to delete a challenge, then redisplays the data
 * so that the change is reflected in the UI.
 * @param {number} id 
 */
async function deleteEntry(id) {
    await deleteEntryAPI(id)
    challengeHandler();
}