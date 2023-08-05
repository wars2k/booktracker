using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;

namespace bookTrackerApi {

    public static class DB {

        public class BookInfo {
            public int? Id { get; set; }
            public string? Title { get; set; }
            public string? Author { get; set; }
            public string? Publisher { get; set; }
            public string? PublishedDate { get; set; }
            
        }
        public static SqliteConnection? connection;

        public static SqliteConnection initiateConnection() {
            string connectionString = "Data Source=external/db/database.db";
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static void InitiateDatabase() {
            using (var connection = new SqliteConnection("Data Source=external/db/database.db"))
            {
                connection.Open();

                // Read the SQL commands from the file
                string script;
                using (var reader = new StreamReader("init.sql"))
                {
                    script = reader.ReadToEnd();
                }

                // Split the script into individual commands
                var commands = script.Split(';');

                // Create a command to execute each command in the script
                foreach (var commandText in commands)
                {
                    if (!string.IsNullOrWhiteSpace(commandText))
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = commandText;
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }

        }

        public static void closeConnection(SqliteConnection connection) {
            connection.Close();
        }


        public static List<BookInfo> getAllBooks() {
            SqliteConnection connection = initiateConnection();
            string sql = "SELECT * FROM books";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    List<BookInfo> bookList = new List<BookInfo>();
                    while (reader.Read()) {
                        BookInfo book = new BookInfo();
                        book.Id = reader.GetInt32(0);
                        book.Title = reader.GetString(1);
                        book.Author = reader.GetString(2);
                        book.Publisher = reader.IsDBNull(4) ? null : reader.GetString(4);
                        book.PublishedDate = reader.IsDBNull(3) ? null : reader.GetString(3);

                        bookList.Add(book);
                    }
                    closeConnection(connection);
                    return bookList;
                }
            }
        }

        public static int addNewEntry(VolumeInfo content) {
            SqliteConnection connection = initiateConnection();
            string sql = "INSERT INTO books (title, author, pub_date, publisher, cover_image, description, page_count, isbn, category) VALUES (@title, @authors, @pubDate, @publisher, @coverImage, @description, @page_count, @isbn, @category)";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@title", content.Title);
            command.Parameters.AddWithValue("@authors", string.Join(", ", content.Authors));
            command.Parameters.AddWithValue("@pubDate", content.PublishedDate);
            command.Parameters.AddWithValue("@publisher", content.Publisher);
            command.Parameters.AddWithValue("@coverImage", content.ImageLinks.Thumbnail != null ? content.ImageLinks?.Thumbnail : "");
            command.Parameters.AddWithValue("@description", content.Description != null? content.Description : "");
            command.Parameters.AddWithValue("@page_count", content.PageCount);
            command.Parameters.AddWithValue("@isbn", content.IndustryIdentifiers != null? content.IndustryIdentifiers[1].Identifier : "");
            command.Parameters.AddWithValue("@category", content.Categories != null ? content.Categories[0] : "");
            command.ExecuteNonQuery();
            closeConnection(connection);
            sql = "SELECT id FROM books WHERE title = @title AND author = @authors";
            int id = -1;
            SqliteConnection connection2 = initiateConnection();
            using (SqliteCommand command2 = new SqliteCommand(sql, connection2)) {
                command2.Parameters.AddWithValue("@title", content.Title);
                command2.Parameters.AddWithValue("@authors", string.Join(", ", content.Authors));
                using (SqliteDataReader reader = command2.ExecuteReader()) {
                    while (reader.Read()) { 
                        id = reader.GetInt32(0);
                    }
                    closeConnection(connection2);
                }
            }
            return id;

        }

        public static int addManualEntry(Api.ManualEntry payload) {
            SqliteConnection connection = initiateConnection();
            string sql = "INSERT INTO books (title, author, pub_date, publisher, cover_image) VALUES (@title, @authors, @pubDate, @publisher, @coverImage)";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@title", payload.Title);
            command.Parameters.AddWithValue("@authors", payload.Author);
            command.Parameters.AddWithValue("@pubDate", payload.Date);
            command.Parameters.AddWithValue("@publisher", payload.Publisher);
            command.Parameters.AddWithValue("@coverImage", payload.Image);
            command.ExecuteNonQuery();
            closeConnection(connection);
            sql = "SELECT id FROM books WHERE title = @title AND author = @authors";
            int id = -1;
            SqliteConnection connection2 = initiateConnection();
            using (SqliteCommand command2 = new SqliteCommand(sql, connection2)) {
                command2.Parameters.AddWithValue("@title", payload.Title);
                command2.Parameters.AddWithValue("@authors", payload.Author);
                using (SqliteDataReader reader = command2.ExecuteReader()) {
                    while (reader.Read()) { 
                        id = reader.GetInt32(0);
                    }
                    closeConnection(connection2);
                }
            }
            return id;
        }

        public static void deleteEntry(string id) {
            SqliteConnection connection = initiateConnection();
            string sql = "DELETE FROM books WHERE id=@id";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
            closeConnection(connection);
        }

        //first, get the current book data from the databse by ID.
        //then, fill in any blank spots in the editBookData with the existing data
        //finally, update the database with the editBookData that has no blank properties.
        public static void editEntry(Api.EditBookData editBookData) {
            SqliteConnection connection = initiateConnection();
            string sql = "SELECT * FROM books WHERE id=@id";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", editBookData.Id);
            SqliteDataReader reader = command.ExecuteReader();
            BookInfo book = new BookInfo();
            while (reader.Read()) {   
                book.Id = reader.GetInt32(0);
                book.Title = reader.GetString(1);
                book.Author = reader.GetString(2);
                book.Publisher = reader.IsDBNull(4) ? null : reader.GetString(4);
                book.PublishedDate = reader.IsDBNull(4) ? null : reader.GetString(3);
            }
            reader.Close();
            if (editBookData.Title == "") {
                editBookData.Title = book.Title;
            }
            if (editBookData.Author == "") {
                editBookData.Author = book.Author;
            }
            if (editBookData.Publisher == "") {
                editBookData.Publisher = book.Publisher;
            }
            if (editBookData.Date == "") {
                editBookData.Date = book.PublishedDate;
            }
            sql = "UPDATE books SET title=@title, author=@author, publisher=@publisher, pub_date=@pubDate WHERE id=@id";
            SqliteCommand command2 = new SqliteCommand(sql, connection);
            command2.Parameters.AddWithValue("@id", editBookData.Id);
            command2.Parameters.AddWithValue("@title", editBookData.Title);
            command2.Parameters.AddWithValue("@author", editBookData.Author);
            command2.Parameters.AddWithValue("@publisher", editBookData.Publisher);
            command2.Parameters.AddWithValue("@pubDate", editBookData.Date);
            command2.ExecuteNonQuery();
            closeConnection(connection);
        }

        public static UserInfo retrieveUserInfo(string username) {
            SqliteConnection connection = initiateConnection();
            string sql = "SELECT * FROM users WHERE username=@username";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);
            SqliteDataReader reader = command.ExecuteReader();
            UserInfo userinfo = new UserInfo();
            while (reader.Read()) {
                userinfo.Id = reader.GetInt32(0).ToString();
                userinfo.Username = reader.GetString(3);
                userinfo.Password = reader.GetString(4);
                userinfo.IsAdmin = reader.GetInt32(5);
            }
            reader.Close();
            closeConnection(connection);
            return userinfo;

        }

        public static Boolean checkForAdminUser() {
            SqliteConnection connection = initiateConnection();
            string sql = "SELECT COUNT(*) AS count_admin_users FROM users WHERE admin = 1";
            SqliteCommand command = new SqliteCommand(sql, connection);
            int count = Convert.ToInt32(command.ExecuteScalar());
            closeConnection(connection);
            return count > 0;
        }

        public static void registerUser(Api.RegisterInfo info) {
            SqliteConnection connection = initiateConnection();
            string sql = "INSERT INTO users (name, email, username, hashed_password, admin) VALUES (@name, @email, @username, @hashed_password, @admin)";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@name", info.Name);
            command.Parameters.AddWithValue("@email", info.Email);
            command.Parameters.AddWithValue("@username", info.Username);
            command.Parameters.AddWithValue("@hashed_password", info.Password);
            command.Parameters.AddWithValue("@admin", info.IsAdmin);
            command.ExecuteNonQuery();
            closeConnection(connection);
        }

        public static List<BookListInfo> getBookListForUser(string UserID) {
            SqliteConnection connection = initiateConnection();
            string sql = "SELECT * FROM book_list2 WHERE idusers = @iduser";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@iduser", UserID);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    List<BookListInfo> bookList = new List<BookListInfo>();
                    while (reader.Read()) {
                        BookListInfo book = new BookListInfo();
                        book.Id = reader.GetInt32(0);
                        book.IdUser = reader.GetInt32(1);
                        book.Username = reader.GetString(2);
                        book.BookID = reader.GetInt32(3);
                        book.Title = reader.GetString(4);
                        book.Author = reader.GetString(5);
                        book.Publisher = reader.IsDBNull(6) ? null : reader.GetString(6);
                        book.PublishedDate = reader.IsDBNull(7) ? null : reader.GetString(7);
                        book.ImageLink = reader.IsDBNull(8) ? null : reader.GetString(8);
                        book.Status = reader.IsDBNull(9) ? null : reader.GetString(9);
                        book.Rating = reader.IsDBNull(10) ? null : reader.GetString(10);
                        book.Thoughts = reader.IsDBNull(11) ? null : reader.GetString(11);
                        book.DateStarted = reader.IsDBNull(12) ? null : reader.GetString(12);
                        book.DateFinished = reader.IsDBNull(13) ? null : reader.GetString(13);
                        bookList.Add(book);
                    }
                    closeConnection(connection);
                    return bookList;
                }
            }
            
        }

        public static void addToBookList(int bookId, string userId) {
            int userIdNumber = int.Parse(userId);
            SqliteConnection connection = initiateConnection();
            string sql = "INSERT INTO user_books (iduser, idbook, status) VALUES (@iduser, @idbook, @status)";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@iduser", userIdNumber);
            command.Parameters.AddWithValue("@idbook", bookId);
            command.Parameters.AddWithValue("@status", "UNASSIGNED");
            command.ExecuteNonQuery();
            closeConnection(connection);
        }

        public static void updateBookList(Api.BookListData data) {
            int? IdNumber = int.Parse(data.Id);
            int? ratingNumber;
            if (data.Rating != null) {
                ratingNumber = int.Parse(data.Rating);
            } else {
                ratingNumber = null;
            }
            
            SqliteConnection connection = initiateConnection();
            string sql = "UPDATE user_books SET ";
            List<SqliteParameter> parameters = new List<SqliteParameter>();

            if (data.Status != null) {
                sql += "status = @status, ";
                parameters.Add(new SqliteParameter("@status", data.Status));
            }
            if (data.Rating != null) {
                sql += "rating = @rating, ";
                parameters.Add(new SqliteParameter("@rating", ratingNumber));
            }
            if (data.StartDate != null) {
                sql += "date_started = @date_started, ";
                parameters.Add(new SqliteParameter("@date_started", data.StartDate));
            }
            if (data.FinishedDate != null) {
                sql += "date_finished = @date_finished ";
                parameters.Add(new SqliteParameter("@date_finished", data.FinishedDate));
            }
            sql = sql.TrimEnd(',', ' ');

            sql += " WHERE iduser_books = @id";
            parameters.Add(new SqliteParameter("@id", IdNumber));
            //Log.sqlQuery(sql);

            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddRange(parameters.ToArray());
            command.ExecuteNonQuery();

            closeConnection(connection);

        }

        public static void deleteFromBookList(string id) {
            SqliteConnection connection = initiateConnection();
            string sql = "DELETE FROM user_books WHERE iduser_books = @id";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
            closeConnection(connection);
        }

        public static BookPageInfo getBookPageData(string id) {
            SqliteConnection connection = initiateConnection();
            string sql = "SELECT * FROM book_list2 WHERE iduser_books = @id";
            BookPageInfo book = new BookPageInfo();
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@id", id);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    
                    while (reader.Read()) {
                        book.Id = reader.GetInt32(0);
                        book.IdUser = reader.GetInt32(1);
                        book.Username = reader.GetString(2);
                        book.BookID = reader.GetInt32(3);
                        book.Title = reader.GetString(4);
                        book.Author = reader.GetString(5);
                        book.Publisher = reader.IsDBNull(6) ? null : reader.GetString(6);
                        book.PublishedDate = reader.IsDBNull(7) ? null : reader.GetString(7);
                        book.ImageLink = reader.IsDBNull(8) ? null : reader.GetString(8);
                        book.Status = reader.IsDBNull(9) ? null : reader.GetString(9);
                        book.Rating = reader.IsDBNull(10) ? null : reader.GetString(10);
                        book.Thoughts = reader.IsDBNull(11) ? null : reader.GetString(11);
                        book.DateStarted = reader.IsDBNull(12) ? null : reader.GetString(12);
                        book.DateFinished = reader.IsDBNull(13) ? null : reader.GetString(13);
                    }
                    closeConnection(connection);
                
                }
            }
            SqliteConnection connection2 = initiateConnection();
            sql = "SELECT * FROM books WHERE id = @id";
            using (SqliteCommand command2 = new SqliteCommand(sql, connection2)) {
                command2.Parameters.AddWithValue("@id", book.BookID);
                using (SqliteDataReader reader = command2.ExecuteReader()) {
                    
                    while (reader.Read()) {
                        book.Description = reader.IsDBNull(6) ? null: reader.GetString(6);
                        book.PageCount = reader.IsDBNull(7) ? null: reader.GetString(7);
                        book.Isbn = reader.IsDBNull(8) ? null: reader.GetString(8);
                        book.Category = reader.IsDBNull(9) ? null: reader.GetString(9);
                    }
                    closeConnection(connection2);
                
                }
            }
            return book;
            
        }

        public static List<Api.UserData> getUserData() {
            SqliteConnection connection = initiateConnection();
            string sql = "SELECT * FROM users";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    List<Api.UserData> userData = new List<Api.UserData>();
                    while (reader.Read()) {
                        Api.UserData user = new Api.UserData();
                        user.Id = reader.GetInt32(0);
                        user.Name = reader.IsDBNull(1) ? null: reader.GetString(1);
                        user.Email = reader.IsDBNull(2) ? null: reader.GetString(2);
                        user.Username = reader.GetString(3);
                        user.IsAdmin = reader.GetInt32(5);

                        userData.Add(user);
                    }
                    closeConnection(connection);
                    return userData;
                }
            }
        }

        public static void createNewUser(Api.NewUserPayload userData) {
            SqliteConnection connection = initiateConnection();
            string sql = "INSERT INTO users (username, hashed_password, admin";
            string valuesSql = " VALUES (@username, @password, @isAdmin";
            
            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.Parameters.AddWithValue("@username", userData.Username);
            command.Parameters.AddWithValue("@password", userData.Password);
            command.Parameters.AddWithValue("@isAdmin", userData.IsAdmin);
            if (userData.Name != null) {
                sql+= ", name";
                valuesSql += ", @name";
                command.Parameters.AddWithValue("@name", userData.Name);
            }
            if (userData.Email != null) {
                sql+= ", email";
                valuesSql += ", @email";
                command.Parameters.AddWithValue("@email", userData.Email);
            }
            sql += ")";
            valuesSql += ")";
            sql = sql + valuesSql;
            command.CommandText = sql;
            command.ExecuteNonQuery();
            closeConnection(connection);
        }

        public static void deleteUser(int id) {
            SqliteConnection connection = initiateConnection();
            string sql = "DELETE FROM users WHERE idusers = @id";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
            closeConnection(connection);
        }

        public static void updateUser(int id, Api.UserData userData) {
            SqliteConnection connection = initiateConnection();
            string sql = "UPDATE users SET ";
            List<SqliteParameter> parameters = new List<SqliteParameter>();

            if (userData.Name != null) {
                sql += "name = @name, ";
                parameters.Add(new SqliteParameter("@name", userData.Name));
            }
            if (userData.Email != null) {
                sql += "email = @email, ";
                parameters.Add(new SqliteParameter("@email", userData.Email));
            }
            if (userData.Username != null) {
                sql += "username = @username, ";
                parameters.Add(new SqliteParameter("@username", userData.Username));
            }
            if (userData.IsAdmin != null) {
                sql += "admin = @admin ";
                parameters.Add(new SqliteParameter("@admin", userData.IsAdmin));
            }
            sql = sql.TrimEnd(',', ' ');

            sql += " WHERE idusers = @id";
            parameters.Add(new SqliteParameter("@id", id));
            //Log.sqlQuery(sql);

            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddRange(parameters.ToArray());
            command.ExecuteNonQuery();

            closeConnection(connection);
        }

        public static List<BookPageInfo> getBookDataForExport(int id) {
            SqliteConnection connection = initiateConnection();
            string sql = "SELECT * FROM book_list2 WHERE idusers = @id";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@id", id);
                using (var reader = command.ExecuteReader()) {
                    List<BookPageInfo> bookListForExport = new List<BookPageInfo>();
                    while (reader.Read()) {
                        BookPageInfo book = new BookPageInfo();
                        book.BookID = reader.GetInt32(3);
                        book.Id = reader.GetInt32(0);
                        book.Title = reader.GetString(4);
                        book.Author = reader.IsDBNull(5) ? null: reader.GetString(5);
                        book.PublishedDate = reader.IsDBNull(6) ? null: reader.GetString(6);
                        book.Publisher = reader.IsDBNull(7) ? null: reader.GetString(7);
                        book.ImageLink = reader.IsDBNull(8) ? null: reader.GetString(8);
                        book.Status = reader.GetString(9);
                        book.Rating = reader.IsDBNull(10) ? null: reader.GetString(10);
                        book.Description = reader.IsDBNull(14) ? null: reader.GetString(14);
                        book.DateStarted = reader.IsDBNull(12) ? null: reader.GetString(12);
                        book.DateFinished = reader.IsDBNull(13) ? null: reader.GetString(13);
                        book.PageCount = reader.IsDBNull(15) ? null: reader.GetString(15);
                        book.Isbn = reader.IsDBNull(16) ? null: reader.GetString(16);
                        book.Category = reader.IsDBNull(17) ? null: reader.GetString(17);
                        bookListForExport.Add(book);
                    }
                    closeConnection(connection);
                    return bookListForExport;
                }
            }
        }

        public static async Task<int> addGoodreadsImportedBook(GoodreadsImportRow book, SessionInfo sessionInfo) {
            string? ImageLink = "";
            string? Description = "";
            string? Categories = "";
            if (book.Title != null) {
                List<object> content = await ApiClient.CallApiAsync(book.Title + " " + book.Author, "12");
                List<VolumeInfoSimple> bookList = content.Cast<bookTrackerApi.VolumeInfoSimple>().ToList();
                
                if (bookList[0].ImageLink != null) {
                    ImageLink = bookList[0].ImageLink;
                }
                if (bookList[0].Description != null) {
                    Description = bookList[0].Description;
                }
                
                if (bookList[0].Categories != null) {
                    Categories = bookList[0].Categories[0];
                }
            }
            string cleanedISBN = Regex.Replace(book.ISBN13, "[^0-9]", "");
            
            SqliteConnection connection = initiateConnection();
            
            string sql = "INSERT INTO books (title, author, pub_date, publisher, cover_image, description, page_count, isbn, category) VALUES (@title, @authors, @pubDate, @publisher, @cover_image, @description, @page_count, @isbn, @category)";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@title", book.Title);
            command.Parameters.AddWithValue("@authors", book.Author);
            command.Parameters.AddWithValue("@pubDate", book.PublishedDate);
            command.Parameters.AddWithValue("@publisher", book.Publisher);
            command.Parameters.AddWithValue("@cover_image", ImageLink);
            command.Parameters.AddWithValue("@description", Description);
            command.Parameters.AddWithValue("@page_count", book.PageCount);
            command.Parameters.AddWithValue("@isbn", cleanedISBN);
            command.Parameters.AddWithValue("@category", Categories);
            command.ExecuteNonQuery();
            closeConnection(connection);
            sql = "SELECT id FROM books WHERE title = @title AND author = @authors";
            int id = -1;
            SqliteConnection connection2 = initiateConnection();
            using (SqliteCommand command2 = new SqliteCommand(sql, connection2)) {
                command2.Parameters.AddWithValue("@title", book.Title);
                command2.Parameters.AddWithValue("@authors", book.Author);
                using (SqliteDataReader reader = command2.ExecuteReader()) {
                    while (reader.Read()) { 
                        id = reader.GetInt32(0);
                    }
                    closeConnection(connection2);
                }
            }
            sql = "INSERT INTO user_books (iduser, idbook, status, rating, thoughts, date_finished) VALUES (@iduser, @idbook, @status, @rating, @thoughts, @date_finished)";
            SqliteConnection connection3 = initiateConnection();
            using (SqliteCommand command3 = new SqliteCommand(sql, connection3)) {
                command3.Parameters.AddWithValue("@iduser", sessionInfo.AssociatedID);
                command3.Parameters.AddWithValue("@idbook", id);
                command3.Parameters.AddWithValue("@status", "UNASSIGNED");
                command3.Parameters.AddWithValue("@rating", book.MyRating);
                command3.Parameters.AddWithValue("@thoughts", book.MyReview);
                command3.Parameters.AddWithValue("@date_finished", book.DateFinished);
                using (SqliteDataReader reader = command3.ExecuteReader()) {
                    while (reader.Read()) { 
                        id = reader.GetInt32(0);
                    }
                    closeConnection(connection3);
                }
            }
            return id;
        }

        public static void updateBookListMetadata(BookListEndpoints.BookListTypes.UpdateRequestBody requestBody) {
            SqliteConnection connection = initiateConnection();
            string sql = "UPDATE books SET title=@title, author=@author, publisher=@publisher, pub_date=@pubDate, cover_image = @coverImage, description = @description, page_count = @pageCount, isbn = @isbn, category = @category WHERE id=@id";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", requestBody.BookID);
            command.Parameters.AddWithValue("@title", requestBody.Title);
            command.Parameters.AddWithValue("@author", requestBody.Author);
            command.Parameters.AddWithValue("@publisher", requestBody.Publisher);
            command.Parameters.AddWithValue("@pubDate", requestBody.datePublished);
            command.Parameters.AddWithValue("@coverImage", requestBody.ImageLink);
            command.Parameters.AddWithValue("@description", requestBody.Description);
            command.Parameters.AddWithValue("@pageCount", requestBody.PageCount);
            command.Parameters.AddWithValue("@isbn", requestBody.Isbn);
            command.Parameters.AddWithValue("@category", requestBody.Category);
            command.ExecuteNonQuery();
            closeConnection(connection);
        }

        public class BookListInfo {
            public int? Id { get; set; }
            public int? IdUser { get; set; }
            public string? Username { get; set; }
            public int? BookID { get; set; }
            public string? Title { get; set; }
            public string? Author { get; set; }
            public string? PublishedDate { get; set; }
            public string? Publisher { get; set; }
            public string? ImageLink { get; set; }
            public string? Status { get; set; }
            public string? Rating { get; set; }
            public string? Thoughts { get; set; }
            public string? DateStarted { get; set; }
            public string? DateFinished { get; set; }
              
        }

        public class UserInfo {
            public string Id { get; set;}
            public string Username { get; set; }
            public string Password { get; set; }
            public int IsAdmin { get; set; }
        }

        public class BookPageInfo {
            public int? Id { get; set; }
            public int? IdUser { get; set; }
            public string? Username { get; set; }
            public int? BookID { get; set; }
            public string? Title { get; set; }
            public string? Author { get; set; }
            public string? PublishedDate { get; set; }
            public string? Publisher { get; set; }
            public string? ImageLink { get; set; }
            public string? Status { get; set; }
            public string? Rating { get; set; }
            public string? Thoughts { get; set; }
            public string? DateStarted { get; set; }
            public string? DateFinished { get; set; }
            public string? Description { get; set; }
            public string? PageCount { get; set; }
            public string? Isbn { get; set; }
            public string? Category { get; set; }
              
        }
    }

}