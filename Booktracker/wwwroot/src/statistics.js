let finishedBooksData;

async function getAllBooks() {
    
    let payload = {
        "sessionKey": localStorage.getItem("sessionKey")
    }
    fetch('/api/getBookList', {
        method: 'PUT',
        body: JSON.stringify(payload),
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (response.status === 401) {
            informIncorrectPassword()
        }
        return response.json()
    })
    .then(data => {
        handleBookListData(data)
    })
    .catch(error => console.error(error));
  }

  async function saveBookListData(data) {
    bookListData = data;
    //console.log(bookListData);
  }
  async function handleBookListData(data, year) {
    let monthArray = displayBooksPerMonth(data);
    let ratingArray = handleRatingData(data);
    let statusData = getStatusData(data);
    let authorData = getAuthorData(data);
    //console.log(authorData);
    //console.log(statusData);
    buildRatingChart(ratingArray);
    buildFinishedChart(monthArray);
    buildStatusChart(statusData);
    buildAuthorChart(authorData);
    
  }

  function displayBooksPerMonth(data) {
    let date = new Date();
    let monthArray = [0,0,0,0,0,0,0,0,0,0,0,0];
    //console.log(data);
    for (let i = 0; i < data.length; i++) {
        const bookData = data[i];
        let finishedDate = bookData.dateFinished;
        if (finishedDate == null || finishedDate == "") {
            continue;
        }
        let dateArray = finishedDate.split("-");
        //console.log(dateArray);
        //console.log(date.getFullYear());
        //console.log(date.getMonth());
        if (dateArray[0] == date.getFullYear()) {
            let index = parseInt(dateArray[1]) -1;
            if (monthArray[index] == null) {
                monthArray[index] = 1;
            } else {
                monthArray[index] += 1;
            }
            
            
        }
    }
    //console.log(monthArray);
    return monthArray
    finishedBooksData = monthArray;
  }

  function handleRatingData(data) {
    let ratingArray = [0,0,0,0,0];
    for (let i = 0; i < data.length; i++) {
        const rating = data[i].rating;
        if (rating == 0) {
            continue
        }
        ratingArray[rating -1] += 1;
        
    }
    return ratingArray;
    buildRatingChart(ratingArray);
  }
  function getStatusData(data) {
    let statusData = {};
    statusData.count = [0,0,0,0,0,0,0];
    statusData.status = ["Unassigned", "Reading", "Up Next", "To Read", "Wishlist", "Finished", "DNF"];
    for (let i = 0; i < data.length; i++) {
        const status = data[i].status;
        switch (status) {
            case "UNASSIGNED":
                statusData.count[0] += 1;
                break;
            case "READING":
                statusData.count[1] += 1;
                break;
            case "UP NEXT":
                statusData.count[2] += 1;
                break;
            case "TO READ":
                statusData.count[3] += 1;
                break;
            case "WISHLIST":
                statusData.count[4] += 1;
                break;
            case "FINISHED":
                statusData.count[5] += 1;
                break;
            case "DNF":
                statusData.count[6] += 1;
                break;
            default:
                break;
        }
        
    }
    return statusData;
  }

  function getAuthorData(data) {
    let authorList = [];
    let multipleList = [];
    
    //get array of lower case author names
    for (let i = 0; i < data.length; i++) {
      const book = data[i];
      let author = book.author;
      authorList.push(author.toLowerCase());
    }
    //get array of nested arrays with number of appearances in index 1
    for (let i = 0; i < authorList.length; i++) {
      let author = authorList[i];
      let authorCount = [author, 0];
      for (let j = 0; j < authorList.length; j++) {
        if (author == authorList[j]) {
          authorCount[1] += 1;
        }
        
      }
      multipleList.push(authorCount);
    }

    //get array that doesn't include authors that only appear once
    let authorCountsMultiple = [];
    for (let i = 0; i < multipleList.length; i++) {
      if (multipleList[i][1] > 1) {
        authorCountsMultiple.push(multipleList[i])
      }
      
    }
    
    //remove duplicates
    const uniqueArrayOfArrays = authorCountsMultiple.filter((arr, index, self) => {
      const stringified = JSON.stringify(arr);
      return index === self.findIndex(subArr => JSON.stringify(subArr) === stringified);
    });

    let sortedArray = uniqueArrayOfArrays.sort((a, b) => b[1] - a[1]);
    let dataForGraphing = createAuthorGraphData(sortedArray);
    return dataForGraphing;
  }

  function createAuthorGraphData(authorArray) {
    let maximumAuthorsToDisplay = 20;
    let AuthorsToDisplay;
    if (authorArray.length < maximumAuthorsToDisplay) {
      AuthorsToDisplay = authorArray.length
    } else {
      AuthorsToDisplay = maximumAuthorsToDisplay;
    }

    let authorGraphData = [];
    for (let i = 0; i < AuthorsToDisplay; i++) {
      let data = {
        x: capitalizeWords(authorArray[i][0]),
        y: authorArray[i][1]
      }
      authorGraphData.push(data);
      
    }
    return authorGraphData
  }

  function capitalizeWords(inputString) {
    return inputString.replace(/\b\w/g, char => char.toUpperCase());
  }


  let yearObjectGlobal

  async function buildFinishedChart(finishedBooksLocalData) {
    var options = {
        chart: {
          type: 'line',
          toolbar: {
            show: false,
            tools: {
                download: false,
                selection: false,
                zoom: false,
                zoomin: false,
                zoomout: false,
                pan: false,
              },
          },
          height: '300px'
        },
        stroke: {
            width: 2,
            curve: 'smooth',
        },
        markers: {
            size: 3
        },
        series: [{
          name: 'Books Read in 2023',
          data: finishedBooksLocalData
        }],
        xaxis: {
          categories: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]
        }
      }
      
      var chart = new ApexCharts(document.querySelector("#chart-demo-line"), options);
      
      chart.render();
  }

  function buildRatingChart(ratings) {
    let totalRates = 0;
    let averageRating;
    ratings.forEach((item => {
        totalRates += item;
    }));
    //let totalRates = ratings[0] + ratings[1] + ratings[2] + ratings[3] + ratings[4] + ratings[5];
    //(ratings);
    if (totalRates > 0) {
      averageRating = ((ratings[0] * 1) + (ratings[1] * 2) + (ratings[2] * 3) + (ratings[3] * 4) + (ratings[4] * 5) )/ totalRates;
      averageRating = averageRating.toFixed(2);
    } else {
      averageRating = 0.00;
    }
    
    //console.log(totalRates);
    //console.log(averageRating);
    
    var options = {
        chart: {
          type: 'bar',
          height: '250px'
        },
        series: [{
          name: 'Books',
          data: [{
            x: "★",
            y: ratings[0]
          }, {
            x: '★★',
            y: ratings[1]
          }, {
            x: '★★★',
            y: ratings[2]
          }, {
            x: '★★★★',
            y: ratings[3]
          }, {
            x: '★★★★★',
            y: ratings[4]
          },
        ]
        }],
        subtitle: {
            text: `In total, you've rated ${totalRates.toString()} books. Your average rating is ${averageRating.toString()}.`,
            offsetY: 5,
            style: {
                fontSize: '14px'
            }
        }
      }
    
      var chart = new ApexCharts(document.querySelector("#ratings-chart"), options);
      
      chart.render();
  }

  function buildStatusChart(data) {
        var options = {
            chart: {
              type: 'donut',
              height: '250px'
            },
            tooltip: {
                show: false
            },
            series: data.count,
            labels: data.status,
            colors:['#f59f00', '#2fb344', '#0ca678', '#0054a6', '#4263eb', '#ae3ec9', '#d63939'],
            plotOptions: {
                pie: {
                  customScale: 1,
                  donut: {
                    size: '40%',
                    labels: {
                      show: true,
                      name: {
                        show: true
                      },
                      total: {
                        show: true,
                        showAlways: true
                      }
                    }
                  }
                }
              }
          }
        
          var chart = new ApexCharts(document.querySelector("#status-chart"), options);
          
          chart.render(); 
  }

  function buildAuthorChart(data) {
    var options = {
      chart: {
        type: 'treemap',
        height: '250px'
      },
      plotOptions: {
        bar: {
          horizontal: true
        }
      },
      series: [{
        data: data
        
      }],
      colors: ['#0ca678']
    }
  
    var chart = new ApexCharts(document.querySelector("#author-chart"), options);
    
    chart.render();
  }
  
  getAllBooks();
  

  
  
