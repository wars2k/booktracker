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
                
            })
            .Produces<List<object>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Books")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Retrieves book objects from the Google Books API."
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
                    int id = DB.addManualEntry(payload);
                    DB.addToBookList(id, currentSession.AssociatedID);
                    return TypedResults.Ok(payload);
                }
                return Results.BadRequest();
            })
            .Accepts<ManualEntry>("application/json")
            .Produces<ManualEntry>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Books")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Manually adds a book to the database."
            });

            //retrieves an array of all Books from the DB to display on the front-end. 
            app.MapGet("/api/books", (string sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    //log failed attempt to access user list
                    return Results.BadRequest();
                }
                if (currentSession.IsAdmin == 0) {
                    //log unauthorized user attempted to access user list
                    return Results.Unauthorized();
                }
                var content = DB.getAllBooks();
                return TypedResults.Ok(content);
            })
            .Produces<DB.BookInfo>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Books")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Retrieves an array of all books in the database."
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
                
            })
            .Produces<VolumeInfo>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Books")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Adds a given book to the database and to the user's bookList by Google Books ID."
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
            })
            .Produces<EditBookData>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Books")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Updates the info for a given book."
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
                
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Books")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Deletes a book from the database and all user's bookLists."
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