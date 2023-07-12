# booktracker
**Booktracker** is an open source application for managing and tracking a personal book library. 

Using metadata grabbed from Google Books, easily add books and organize them into collections. Set each book’s status, give it a rating, and track how long it took to read. 

**Disclaimer:** I am an English Lit graduate learning to program, and Booktracker is my first major project. Booktracker is still under active development, so expect bugs and breaking changes.

![Collection Page](/screenshots/collections.JPG)

## Features

- Add books with metadata provided by the Google Books API.
- Add books manually without using an external metadata provider.
- Organize books into collections.
- Multi-user support.
- Import book data and bookshelves from Goodreads.
- Export data to JSON or CSV.

## Installation

**Docker Compose (recommended)**

1. Paste this `docker-compose.yml` file into an empty directory, replacing with the correct info where necessary
    
    ```yaml
    version: "3.3"

    services:
        booktracker:
            image: wars2k/booktracker:v0.2-beta
            restart: unless-stopped
            volumes:
                - ./data:/app/external
            ports:
                - 2341:5000 #replace 2341 with your desired port.
    ```
    
2. Create the `data` directory with the following three subdirectories: 
    - `db`
    - `logs`
    - `export`

   Before starting the container, make sure that the directory strucutre looks like this: 
    ```
   booktracker/
    ├── docker-compose.yml
    └── data/
        ├── db
        ├── log
        └── export
    ```
    
3. Start the container (from the same directory as your `docker-compose.yml` file): 
    
    ```bash
    docker compose up -d
    ```

## Coming Soon

**Features**

- **Format**: Choose from options like `ebook`, `paperback`, `hardback`, `missing` to further categorize your books.
- **Journal/Reviews:** Leave multiple private journal entries about a book, or write a review.
- **Statistics:** View book-related statistics like books read per month, average time to completion, etc.
- **Social Media:** Enable this optional feature to view all users’ activity (adding books, finishing books, writing reviews, etc.) on a timeline.
- **Email Summaries:** Receive weekly or monthly recaps with statistics on books read, started, finished, etc.

**Boring Stuff**

- **Improved Logs**
- **Improved Error Handling**

## Screenshots

**Home Page**

![Home Page](/screenshots/home.JPG)

**Main Book Page**

![Main Book Page](/screenshots/bookList.JPG)

**Settings**

![Settings](/screenshots/settings.JPG)

**Users**

![Users](/screenshots/Users.JPG)
 
