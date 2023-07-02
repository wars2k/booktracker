using Newtonsoft.Json;
using System.Text;

namespace bookTrackerApi {

    public static class Api {

        public static void configure(WebApplication app) {
            app.UseCors(builder =>
                builder.WithOrigins("http://localhost:5500")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("Access-Control-Allow-Origin")
            );
        }

        
        public static void configureBookEndpoints(WebApplication app) {

            //receives a title from the front-end as "name", then searches for that title in the Google Books API. 
            //the Google Books API returns the top 5 results, which are then returned to the front-end to be displayed.
            app.MapGet("/api/books/new", async (string name, string sessionKey) => {
                if (name == null || sessionKey == null) {
                    Log.failedAPIquery("null", "missingParameter");
                    return Results.BadRequest("Parameters missing. Required Parameters: string Name & string sessionKey.");
                }
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest();
                }
                if (sessionKey != currentSession.Session) {
                    Log.failedAPIquery(name, "incorrectSessionKey");
                    return Results.Unauthorized();
                }
                var content = await ApiClient.CallApiAsync(name);
                Log.externalAPIquery(name, currentSession);
                return Results.Ok(content);
                
            });

            app.MapPost("/api/books/new/manual", async (HttpContext context, string sessionKey) => {
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<ManualEntry>(requestBody);
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    Log.failedManualEntry("incorrectSessionKey", null);
                    return TypedResults.Unauthorized();
                    
                }
                if (requestBody == null) {
                    Log.failedManualEntry("noRequestBody", currentSession);
                    return TypedResults.BadRequest("no request body");
                }
                if (payload != null && payload.Title != null) {
                    Log.logManualEntry(payload.Title, currentSession);
                    DB.addManualEntry(payload);
                    return TypedResults.Ok(payload);
                }
                return Results.BadRequest();
            });

            //retrieves an array of all Books from the DB to display on the front-end. 
            app.MapGet("/api/books", () => {
                var content = DB.getAllBooks();
                return TypedResults.Ok(content);
            });

            //receives an ID from the front end. This ID is searched on Google Books API, and the result is added to the Database. 
            app.MapPost("/api/books/save", async (string id, string sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    string message = $"Attempt to save Google Books ID {id} failed due to incorrect session key.";
                    Log.writeLog(message, "ERROR");
                    return Results.Unauthorized();
                }
                VolumeInfo book = await ApiClient.GetBookFromID(id);
                if (book == null) {
                    return Results.BadRequest("book not found");
                }
                int bookId = DB.addNewEntry(book);
                Log.writeLog($"'{book.Title}' added by user '{currentSession.Username}'", "INFO");
                DB.addToBookList(bookId, currentSession.AssociatedID);
                return Results.Ok(book);
                
            });

            //receives an ID along with the desired updates to an entry. The updates are compiled into an EditBookData instance,
            //and then the updates are sent to the database. 
            app.MapPut("/api/books/{id}", async (string id, string title, string author, string publisher, string date, string sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    string message = $"Attempt to update book ID {id} failed due to incorrect session key.";
                    Log.writeLog(message, "ERROR");
                    return Results.Unauthorized();
                }
                if (currentSession.IsAdmin == 0) {
                    string message = $"Attempt to update book ID {id} failed due to no admin privileges.";
                    Log.writeLog(message, "ERROR");
                    return Results.Unauthorized();
                }
                EditBookData editInfo = new EditBookData();
                editInfo.Id = id;
                editInfo.Title = title;
                editInfo.Author = author;
                editInfo.Publisher = publisher;
                editInfo.Date = date;
                DB.editEntry(editInfo);
                return Results.Ok(editInfo);
            });

            //receives an ID for a given book. The ID is passed along to a function that deletes it from the database.
            app.MapDelete("/api/books/{id}/delete", (string id, string sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest();
                }
                if (currentSession.IsAdmin == 1) {
                    Log.logDeletedBook(id, currentSession);
                    DB.deleteEntry(id);
                    return Results.Ok("Book succesfully deleted.");
                } else {
                    return Results.Unauthorized();
                }
                
            });

            //receives a username and password. Checks that with the database of users.
            //if both are correct, starts a new session and returns the session key to the client.
            app.MapPost("/api/login", async (HttpContext context) => {
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<UserInfo>(requestBody);
                if (payload == null) {
                    return Results.BadRequest();
                }
                if (payload.Username == null || payload.Password == null) {
                    return Results.BadRequest();
                }
                DB.UserInfo userInfo = DB.retrieveUserInfo(payload.Username);
                if (userInfo.Password == payload.Password) {
                    string generateSession = generateSessionKey(32);
                    SessionInfo newSession  = new SessionInfo();
                    newSession.Session = generateSession;
                    newSession.AssociatedID = userInfo.Id;
                    newSession.Username = userInfo.Username;
                    newSession.IsAdmin = userInfo.IsAdmin;
                    Program.Sessions.Add(newSession);
                    Log.logSuccessfulLoginAttempt(payload.Username);
                    return Results.Ok(generateSession);
                } else {
                    Log.logFailedLoginAttempt(payload.Username);
                    return Results.Unauthorized();
                }   
            });

            //logs out the user on the server-side by nullifying the sessionKey & associatedID
            app.MapPost("/api/logout", (string sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession != null) {
                    Log.logSuccessfulLogout(currentSession);
                    Program.Sessions.Remove(currentSession);
                }
                return Results.Ok();
            });

            app.MapGet("/api/register/canRegister", () => {
                Boolean adminExists = DB.checkForAdminUser();
                if (adminExists) {
                    return Results.Unauthorized();
                } else {
                    return Results.Ok();
                }
            });

            app.MapPost("/api/register", async (HttpContext context) => {
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<RegisterInfo>(requestBody);
                if (payload == null) {
                    return Results.BadRequest();
                }
                DB.registerUser(payload);
                return Results.Ok();
            });

            //retrieves a user's bookList based on user ID if provided sessionkey is correct
            app.MapPut("/api/getBookList", async (HttpContext context) => {
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<BookListRequestBody>(requestBody);
                if (payload == null) {
                    Log.AlertFailedBookListRetrieval("emptyPayload", null);
                    return Results.BadRequest("emptyPayload");
                }
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == payload.SessionKey);
                if (currentSession == null) {
                    Log.AlertFailedBookListRetrieval("noBackEndSession", null);
                    return Results.BadRequest("noBackEndSession");
                }
                if (payload.SessionKey == currentSession.Session) {
                    List<DB.BookListInfo> bookList = DB.getBookListForUser(currentSession.AssociatedID);
                    return Results.Ok(bookList);
                }
                Log.AlertFailedBookListRetrieval("incorrectSessionKey", null);
                return Results.Unauthorized();
            });

            //updates an entry for a given userID's booklist
            app.MapPut("/api/BookList/{id}", async (HttpContext context) => {
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<BookListEdit>(requestBody);
                if (payload == null) {
                    Log.AlertFailedBookListEdit("emptyPayload", null);
                    return Results.BadRequest();
                }
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == payload.SessionKey);
                if (currentSession == null) {
                    Log.AlertFailedBookListEdit("noBackEndSession", null);
                    return Results.BadRequest();
                }
                if (payload.SessionKey == currentSession.Session && payload.Data != null) {
                    DB.updateBookList(payload.Data);
                    Log.AlertSuccessfulBookListEdit(payload.Data, currentSession);
                    return Results.Ok();
                }
                if (payload.SessionKey != currentSession.AssociatedID) {
                    Log.AlertFailedBookListEdit("incorrectSessionKey", currentSession);
                    return Results.Unauthorized();
                }
                Log.AlertFailedBookListEdit("unknownError", currentSession);
                return Results.BadRequest();
                
            });

            //deletes a book from a user's booklist, but does not delete it from the main book database
            //requires id in URL and sessionKey in body
            app.MapDelete("/api/Booklist/{id}/delete", async (String id, HttpContext context) => {
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<BookListRequestBody>(requestBody);
                if (payload == null) {
                    Log.AlertFailedBookListDelete("emptyPayload", null);
                    return Results.BadRequest();
                }
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == payload.SessionKey);
                if (currentSession == null) {
                    Log.AlertFailedBookListDelete("noBackEndSession", null);
                    return Results.BadRequest();
                }
                if (payload.SessionKey == currentSession.Session) {
                    DB.deleteFromBookList(id);
                    Log.AlertSuccessfulBookListDelete(id, currentSession);
                    return Results.Ok();
                }
                if (payload.SessionKey != currentSession.AssociatedID) {
                    Log.AlertFailedBookListDelete("incorrectSessionKey", currentSession);
                    return Results.Unauthorized();
                }
                Log.AlertFailedBookListDelete("unknownError", currentSession);
                return Results.BadRequest();
            });

            //retreives all of the data for a given booklist ID
            //requires id in URL and sessionKey in body
            app.MapPut("/api/Booklist/{id}/data", async (String id, HttpContext context) => {
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<BookListRequestBody>(requestBody);
                if (payload == null) {
                    return Results.BadRequest();
                }
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == payload.SessionKey);
                if (currentSession == null) {
                    return Results.BadRequest();
                }
                if (payload.SessionKey == currentSession.Session) {
                    DB.BookPageInfo data = DB.getBookPageData(id);
                    return Results.Ok(data);
                }
                if (payload.SessionKey != currentSession.Session) {
                    return Results.Unauthorized();
                }
                return Results.BadRequest(); 
            });

            app.MapPost("/api/users/new", async (String sessionKey, HttpContext context) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    //log failed attempt to create new user
                    return Results.BadRequest();
                }
                if (currentSession.IsAdmin == 0) {
                    //log unauthorized user attempted to add new user
                    return Results.Unauthorized();
                }
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<NewUserPayload>(requestBody);
                if (payload == null) {
                    return Results.BadRequest();
                }
                if (payload.Username == null || payload.Password == null) {
                    return Results.BadRequest("Must include new username & password in request body");
                }
                DB.createNewUser(payload);
                Log.writeLog($"New user ({payload.Username}) created by admin user {currentSession.Username}.", "INFO");
                return Results.Ok();
            });

            app.MapGet("/api/users", (String sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    //log failed attempt to access user list
                    return Results.BadRequest();
                }
                if (currentSession.IsAdmin == 0) {
                    //log unauthorized user attempted to access user list
                    return Results.Unauthorized();
                }
                List<UserData> userData = DB.getUserData();
                return Results.Ok(userData);
            })
            .Produces<List<UserData>>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags("Users")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Retrieves a list of all users."
            });

            app.MapPut("/api/users/{id}", async (String id, String sessionKey, HttpContext context) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    //log failed attempt to create new user
                    return Results.BadRequest();
                }
                if (currentSession.IsAdmin == 0) {
                    //log unauthorized user attempted to add new user
                    return Results.Unauthorized();
                }
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<UserData>(requestBody);
                if (payload == null) {
                    return Results.BadRequest();
                }
                DB.updateUser(int.Parse(id), payload);
                return Results.Ok();
            });

            app.MapDelete("/api/users/{id}", async (String id, String sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest();
                }
                if (currentSession.IsAdmin == 0) {
                    return Results.Unauthorized();
                }
                DB.deleteUser(int.Parse(id));
                return Results.Ok();
            });

            app.MapGet("/api/data/export", async (String format, String sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest();
                }
                List<DB.BookPageInfo> ListOfBookData = DB.getBookDataForExport(int.Parse(currentSession.AssociatedID));
                if (format == "json") {
                    Export.ExportDataAsJSON(ListOfBookData, currentSession.Username);
                } else if (format == "csv") {
                    Export.ExportDataAsCSV(ListOfBookData, currentSession.Username);
                } else {
                    return Results.BadRequest();
                }
                byte[] test = File.ReadAllBytes($"external/{currentSession.Username}-export.{format}");
                return Results.File(test, "text/csv", $"bookExport.{format}");
            });

            app.MapPost("/api/data/import", async (String format, String sessionKey, IFormFile file) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest();
                }
                if (format == "goodreads") {
                    Import.ImportFromGoodreads(file, currentSession);
                }
                return Results.Ok();
                //read the file
            });

            app.MapGet("/api/test/test", async () => {
                return Results.Ok("test");
            });

            app.MapPut("/api/settings", async (String results, String sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest();
                }
                ApiClient.Results = int.Parse(results);
                return Results.Ok();
            });


        }

        public class EditBookData {
            public string? Id { get; set; }
            public string? Title { get; set; }
            public string? Author { get; set; }
            public string? Publisher { get; set; } 
            public string? Date { get; set; }
        }

        public class ManualEntry {
            public string? Id { get; set; }
            public string? Title { get; set; }
            public string? Author { get; set; }
            public string? Publisher { get; set; } 
            public string? Date { get; set; }
            public string? Image { get; set; }
        }

        public class UserInfo {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        public class RegisterInfo {
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? IsAdmin { get; set; }
        }

        public class BookListRequestBody {
            public string? SessionKey { get; set; }
        }

        public class BookListEdit {

            public string? SessionKey { get; set; }
            public BookListData? Data { get; set; }
        }

        public class BookListData {
            public string? Id { get; set; }
            public string? Rating { get; set; }
            public string? Status { get; set; }
            public string? StartDate { get; set; }
            public string? FinishedDate { get; set; }
        }

        public class NewUserPayload {
            public string? Name { get; set; }
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? Password { get; set; }
            public string? IsAdmin { get; set; }
        }

        public class UserData {
            public int? Id { get; set; }
            public string? Name { get; set; }
            public string? Username { get; set; }
            public string? Email { get; set; }
            public int? IsAdmin { get; set; }
        }

        public static string generateSessionKey(int length) {

            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var stringBuilder = new StringBuilder(length);

            for (int i = 0; i < length; i++) {
    
                int randomIndex = random.Next(validChars.Length);
                stringBuilder.Append(validChars[randomIndex]);
            }

            return stringBuilder.ToString();
        }
        
    }
}