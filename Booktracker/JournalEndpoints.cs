using Newtonsoft.Json;
using System.Text;

namespace bookTrackerApi {

    public static class JournalEndpoints {

        public static void Configure(WebApplication app) {

            app.MapGet("/api/journal/{bookID}/entries", async (HttpContext context, string sessionKey, string bookID) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "journal_view", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                DB.BookPageInfo data = DB.getBookPageData(bookID);
                if (data.IdUser.ToString() != currentSession.AssociatedID) {
                    JsonLog.writeLog("Unauthorized attempt to access entries for another user's book.","WARNING", "journal_view",currentSession,remoteIp);
                    return Results.Unauthorized();
                }
                List<JournalTypes.JournalEntryList> entries = JournalDB.getJournalEntries(bookID, currentSession.AssociatedID);
                return Results.Ok(entries);
            })
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<List<JournalTypes.JournalEntryList>>(StatusCodes.Status200OK)
            .WithTags("Journal")
            .WithOpenApi(operation => new(operation) {
                Summary = "Retrieves all journal entries for a given bookList ID."
            });

            app.MapGet("/api/journal/{journalID}", async (HttpContext context, string sessionKey, string journalID) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "journal_viewOne", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                JournalTypes.JournalEntryList entry = JournalDB.getEntry(journalID);
                if (entry.id == null) {
                    return Results.Ok("deleted entry");
                }
                DB.BookPageInfo data = DB.getBookPageData(entry.idBookList.ToString());
                if (data.IdUser.ToString() != currentSession.AssociatedID) {
                    JsonLog.writeLog("Unauthorized attempt to access entries for another user's book.","WARNING", "journal_view",currentSession,remoteIp);
                    return Results.Unauthorized();
                }
                return Results.Ok(entry);
            })
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<JournalTypes.JournalEntryList>(StatusCodes.Status200OK)
            .WithTags("Journal")
            .WithOpenApi(operation => new(operation) {
                Summary = "Retrieves a specific journal entry by ID."
            });

            app.MapPost("/api/journal/{bookID}/entries", async (HttpContext context, string sessionKey, string bookID) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "journal_createEntry", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<JournalTypes.NewEntry>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.missing_request_body, "journal_createEntry", currentSession, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                if (payload.title == null || payload.htmlContent == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_request_body, "journal_createEntry", currentSession, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                DB.BookPageInfo data = DB.getBookPageData(bookID);
                if (data.IdUser.ToString() != currentSession.AssociatedID) {
                    JsonLog.writeLog("Unauthorized attempt to create an entry for another user's book.","WARNING", "journal_createEntry",currentSession,remoteIp);
                    return Results.Unauthorized();
                }
                int lastInserted = JournalDB.createEntry(payload, currentSession, bookID);
                ChallengeDB.handleChallenges(currentSession.AssociatedID, "writing", "", lastInserted);
                EventTypes.Internal journalEvent = new EventTypes.Internal(Int32.Parse(currentSession.AssociatedID), Int32.Parse(bookID), EventTypes.EventCategories.journal, lastInserted.ToString());
                EventDB.Add(journalEvent);
                return Results.Ok(lastInserted);

            })
            .Accepts<JournalTypes.NewEntry>("application/json")
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status200OK)
            .WithTags("Journal")
            .WithOpenApi(operation => new(operation) {
                Summary = "Creates a new journal entry for a given bookList ID."
            });

            app.MapPut("/api/journal/{bookID}/entries/{journalID}", async (HttpContext context, string sessionKey, string bookID, string journalID) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "journal_updateEntry", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<JournalTypes.NewEntry>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.missing_request_body, "journal_updateEntry", currentSession, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                if (payload.title == null || payload.htmlContent == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_request_body, "journal_updateEntry", currentSession, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                DB.BookPageInfo data = DB.getBookPageData(bookID);
                if (data.IdUser.ToString() != currentSession.AssociatedID) {
                    JsonLog.writeLog("Unauthorized attempt to update an entry for another user's book.","WARNING", "journal_updateEntry",currentSession,remoteIp);
                    return Results.Unauthorized();
                }
                JournalDB.updateEntry(payload, journalID);
                return Results.Ok();
            })
            .Accepts<JournalTypes.NewEntry>("application/json")
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status200OK)
            .WithTags("Journal")
            .WithOpenApi(operation => new(operation) {
                Summary = "Updates a journal entry by ID for a given bookList ID."
            });

            app.MapDelete("/api/journal/{bookID}/entries/{journalID}", async (HttpContext context, string sessionKey, string bookID, string journalID) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "journal_deleteEntry", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                DB.BookPageInfo data = DB.getBookPageData(bookID);
                if (data.IdUser.ToString() != currentSession.AssociatedID) {
                    JsonLog.writeLog("Unauthorized attempt to delete an entry for another user's book.","WARNING", "journal_deleteEntry",currentSession,remoteIp);
                    return Results.Unauthorized();
                }
                JournalDB.deleteEntry(journalID);
                return Results.Ok();
            })
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status200OK)
            .WithTags("Journal")
            .WithOpenApi(operation => new(operation) {
                Summary = "Deletes a journal entry by ID for a given bookList ID."
            });

        }

    }

    public static class JournalTypes {

        public class JournalEntryList {
            public int? id { get; set; }
            public int? idUser { get; set; }
            public int? idBookList { get; set; }
            public string? dateCreated { get; set; }
            public string? lastEdited { get; set; }
            public string? title { get; set; }
            public string? htmlContent { get; set; }
        }

        public class NewEntry {
            public string? title { get; set; }
            public string? htmlContent { get; set; }
        }
    }
}