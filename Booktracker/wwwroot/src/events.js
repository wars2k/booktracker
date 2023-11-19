eventDisplayHandler();

/**
 * Handler function that builds the table of events. 
 * @returns null
 */
async function eventDisplayHandler() {
    let table = document.getElementById("eventTable");
    let events = await getEvents();
    if (events.length == 0) {
        return;
    }
    let currentDate;
    for (let i = 0; i < events.length; i++) {
        let event = events[i];
        let object = await getEventObject(event);
        
        if (i == 0) {
            currentDate = object.dateString;
        }

        if (currentDate != object.dateString) {
            let divider = getDivider(currentDate);
            table.prepend(divider);
            currentDate = object.dateString;
        }

        table.prepend(object.row);
    }

    let divider = getDivider(currentDate);
    table.prepend(divider);


}

/**
 * Gets all events for the current page's book from the server.
 * @returns An array of events from the server.
 */
async function getEvents() {
    let sessionKey = localStorage.getItem("sessionKey");
      
        return fetch(`/api/BookList/${getBookIDfromURL()}/events?sessionKey=${sessionKey}`, {
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
 * For a given event in the API response from the server, creates the correct object and builds the HTML for the row. 
 * @param {*} event A response from the event api endpoint.
 * @returns The correct object based on the event type.
 */
async function getEventObject(event) {
    let object;
    switch (event.eventType) {
        case "added":
            object = new AddedEvent(event);
            break;
        case "ratingUpdate":
            object = new RatingEvent(event);
            break;
        case "statusUpdate":
            object = new StatusEvent(event);
            break;
        case "journal":
            object = new JournalEvent(event);
            break;
        case "dateStartedUpdate":
            object = new StartDateEvent(event);
            break;
        case "dateFinishedUpdate":
            object = new FinishDateEvent(event);
            break;
        case "progress":
            object = new ProgressEvent(event);
            break;
        default:
            break;
    }
    await object.buildRow();
    return object;
}

/**
 * For a given date, creates and returns a row with colspan 2 that can be displayed as a divider
 * on the event table.
 * @param {string} dateString A date in the format mm/dd/yyyy
 * @returns An HTML "tr" element that displays a divider.
 */
function getDivider(dateString) {
    let row = document.createElement("tr");
    let data = document.createElement("td");
    data.colSpan = "2";
    row.append(data);
    let divider = document.createElement("div");
    divider.classList.add("hr-text", "dateDivider");
    divider.style.fontSize = "10pt";
    divider.innerText = dateString;
    data.append(divider);

    return row;
}

/**
 * Called when one of the filter buttons is clicked on the event table. 
 * First it removes all active filters. Then, it loops through all events, hiding
 * each one that that is not part of the active filter set. Then, it loops through
 * all rows in the table. For each divider, it determines whether or not that
 * divider should still appear. 
 * @returns null
 */
function filterByType() {
    let filterArray = getFilterArray()

    removeFilter()

    if (filterArray.length == 0) {
        return;
    }
    
    let events = document.getElementsByClassName("event");
    for (let i = 0; i < events.length; i++) {
        const event = events[i];
        
        if (filterArray.includes(event.children[0].children[1].innerText)) {
            event.style.display = "";
        } else {
            event.style.display = "none";
        }
        
    }

    let table = document.getElementById("eventTable");
    let rows = table.children;

    for (let i = 0; i < rows.length; i++) {
        let row = rows[i];

        //if this is a divider row
        if (row.children[0].colSpan == "2") {
            let nextDiv = null;
            let nextEvent = rows.length;

            //look for the next divider row and the next visible event row
            for (let j = rows.length - 1; j > i; j--) {
                
                if (rows[j].children[0].colSpan == "2") {
                    nextDiv = j;
                    continue;
                }

                if (rows[j].style.display != "none") {
                    nextEvent = j;
                }
                
            }

            //if the next div happens before the next visible event, hide the div.
            if (nextDiv != null && nextDiv < nextEvent) {

                row.style.display = "none";
            }

            //if there are no more divs AND no more rows coming up, hide the current div. 
            if (nextDiv == null && nextEvent == rows.length) {
                
                row.style.display = "none";
            }

            
            
        }
    }


}

/**
 * Creates an array of active filters based on the check list in the filter drop-down
 * on the event table. 
 * @returns An array of active filters
 */
function getFilterArray() {
    let filterArray = [];
    for (const child of document.getElementById("typeFilter").children) {
      if (child.children[0].checked == 1) {
        filterArray.push(child.children[0].value);
      }
    }
    return filterArray;
}

/**
 * Loops through each row of the event table, making sure each row is set to appear. 
 */
function removeFilter() {
    let events = document.getElementById("eventTable").children;
    for (let i = 0; i < events.length; i++) {
        const event = events[i];
        event.style.display = "";
        
    }
}