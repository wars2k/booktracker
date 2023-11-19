CREATE TABLE IF NOT EXISTS `authors` (
  `idauthors` INTEGER PRIMARY KEY AUTOINCREMENT,
  `name` TEXT,
  `nationality` TEXT
);

-- Table structure for table `books`
-- DROP TABLE `books`;

CREATE TABLE IF NOT EXISTS `books` (
  `id` INTEGER PRIMARY KEY AUTOINCREMENT,
  `title` TEXT NOT NULL,
  `author` TEXT,
  `pub_date` TEXT,
  `publisher` TEXT,
  `cover_image` TEXT,
  `description` TEXT,
  `page_count` TEXT,
  `isbn` TEXT,
  `category` TEXT
);

CREATE TABLE IF NOT EXISTS `settings` (
  `name` TEXT PRIMARY KEY,
  `value` TEXT NOT NULL
);

INSERT OR IGNORE INTO `settings` ('name', 'value')
VALUES ('logging_level', 'all');

-- Table structure for table `users`
CREATE TABLE IF NOT EXISTS `users` (
  `idusers` INTEGER PRIMARY KEY AUTOINCREMENT,
  `name` TEXT,
  `email` TEXT,
  `username` TEXT NOT NULL,
  `hashed_password` TEXT,
  `admin` INTEGER DEFAULT 0
);

-- Table structure for table `user_books`

--DROP TABLE `user_books`;

CREATE TABLE IF NOT EXISTS `user_books` (
  `iduser_books` INTEGER PRIMARY KEY AUTOINCREMENT,
  `iduser` INTEGER NOT NULL,
  `idbook` INTEGER NOT NULL,
  `status` TEXT NOT NULL,
  `rating` INTEGER,
  `thoughts` TEXT,
  `date_started` TEXT,
  `date_finished` TEXT,
  FOREIGN KEY (`iduser`) REFERENCES `users` (`idusers`) ON DELETE CASCADE,
  FOREIGN KEY (`idbook`) REFERENCES `books` (`id`) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS `collections` (
  `idcollection` INTEGER PRIMARY KEY AUTOINCREMENT,
  `collection_name` TEXT NOT NULL,
  `collection_description` TEXT,
  `collection_cover_image` TEXT,
  `userID` INTEGER NOT NULL,
  `dateTime` TEXT,
  FOREIGN KEY (`userID`) REFERENCES `users` (`idusers`) ON DELETE CASCADE 
);

CREATE TABLE IF NOT EXISTS `collection_entries` (
  `identry` INTEGER PRIMARY KEY AUTOINCREMENT,
  `idcollection` INTEGER NOT NULL,
  `iduser_book` INTEGER NOT NULL,
  `dateTime` TEXT,
  FOREIGN KEY (`idcollection`) REFERENCES `collections` (`idcollection`) ON DELETE CASCADE,
  FOREIGN KEY (`iduser_book`) REFERENCES `user_books` (`iduser_books`) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS `journal_entries` (
  `id` INTEGER PRIMARY KEY AUTOINCREMENT,
  `iduser` INTEGER NOT NULL,
  `idbooklist` INTEGER NOT NULL,
  `date_created` TEXT,
  `last_edited` TEXT,
  `title` TEXT NOT NULL,
  `html_content`,
  FOREIGN KEY (`iduser`) REFERENCES `users` (`idusers`) ON DELETE CASCADE,
  FOREIGN KEY (`idbooklist`) REFERENCES `user_books` (`iduser_books`) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS 'challenges' (
  `id` INTEGER PRIMARY KEY AUTOINCREMENT,
  `iduser` INTEGER NOT NULL,
  `title` TEXT NOT NULL,
  `description` TEXT,
  `date_created` TEXT NOT NULL,
  `status` TEXT,
  `type` TEXT NOT NULL,
  `subtype` TEXT NOT NULL,
  `start_date` TEXT NOT NULL,
  `end_date` TEXT NOT NULL,
  `goal` INTEGER NOT NULL,
  `count` INTEGER NOT NULL DEFAULT 0,
  `record` TEXT,
  FOREIGN KEY (`iduser`) REFERENCES `users` (`idusers`) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS 'book_events' (
  'id' INTEGER PRIMARY KEY AUTOINCREMENT,
  'iduser' INTEGER NOT NULL,
  'idbookList' INTEGER NOT NULL,
  'dateTime' TEXT NOT NULL,
  'event' TEXT NOT NULL,
  'value' TEXT,
  FOREIGN KEY ('iduser') REFERENCES 'users' ('idusers') ON DELETE CASCADE,
  FOREIGN KEY ('idbookList') REFERENCES 'user_books' ('iduser_books') ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS 'progress_updates' (
  'id' INTEGER PRIMARY KEY AUTOINCREMENT,
  'iduser' INTEGER NOT NULL,
  'idbookList' INTEGER NOT NULL,
  'dateTime' TEXT NOT NULL,
  'currentPosition' INTEGER NOT NULL,
  'journalID' INTEGER DEFAULT NULL,
  'comment' TEXT,
  FOREIGN KEY ('iduser') REFERENCES 'users' ('idusers') ON DELETE CASCADE,
  FOREIGN KEY ('idbookList') REFERENCES 'user_books' ('iduser_books') ON DELETE CASCADE,
  FOREIGN KEY ('journalID') REFERENCES 'journal_entries' ('id') ON DELETE SET NULL
);

CREATE VIEW IF NOT EXISTS book_list2 AS
SELECT
    t1.iduser_books AS iduser_books, 
    t2.idusers AS idusers,
    t2.username AS username,
    t3.id AS id,
    t3.title AS title,
    t3.author AS author,
    t3.pub_date AS pub_date,
    t3.publisher AS publisher,
    t3.cover_image AS cover_image,
    t1.status AS status,
    t1.rating AS rating,
    t1.thoughts AS thoughts,
    t1.date_started AS date_started,
    t1.date_finished AS date_finished,
    t3.description AS description,
    t3.page_count AS page_count,
    t3.isbn AS isbn,
    t3.category AS category
FROM
    user_books t1
    JOIN users t2 ON t1.iduser = t2.idusers
    JOIN books t3 ON t1.idbook = t3.id;
