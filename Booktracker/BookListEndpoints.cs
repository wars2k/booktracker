using Newtonsoft.Json;
using System.Text;

namespace bookTrackerApi {

    public static class BookListEndpoints {

        public static void configure(WebApplication app) {

            //retrieves a user's bookList based on user ID if provided sessionkey is correct
            app.MapPut("/api/getBookList", async (HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<Api.BookListRequestBody>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.missing_request_body, "booklist_retrieval", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == payload.SessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "booklist_retrieval", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                if (payload.SessionKey == currentSession.Session) {
                    List<DB.BookListInfo> bookList = DB.getBookListForUser(currentSession.AssociatedID);
                    return Results.Ok(bookList);
                }
                return Results.Unauthorized();
            })
            .Accepts<Api.BookListRequestBody>("application/json")
            .Produces<List<DB.BookListInfo>>(StatusCodes.Status200OK)
            .Produces<ErrorMessage>(StatusCodes.Status500InternalServerError)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .WithTags("Book List")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Retrieves a list of all of a given user's books.",
                Description = "Currently this endpoint is a PUT when it should be a GET."
            });


            //updates an entry for a given userID's booklist
            app.MapPut("/api/BookList/{id}", async (HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<Api.BookListEdit>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.missing_request_body, "booklist_update", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == payload.SessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "booklist_update", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                if (payload.SessionKey == currentSession.Session && payload.Data != null) {
                    DB.updateBookList(payload.Data);
                    EventDB.HandleBookListEvents(payload.Data, Int32.Parse(currentSession.AssociatedID), Int32.Parse(payload.Data.Id));
                    if (payload.Data.Status == "READING") {
                        ChallengeDB.handleChallenges(currentSession.AssociatedID, "reading", "1", Int32.Parse(payload.Data.Id));
                    }
                    if (payload.Data.Status == "FINISHED") {
                        ChallengeDB.handleChallenges(currentSession.AssociatedID, "reading", "2", Int32.Parse(payload.Data.Id));
                    }
                    JsonLog.writeLog("Booklist Entry has been succesfully updated", "INFO", "booklist_update", currentSession, remoteIp);
                    return Results.Ok();
                }
                return Results.BadRequest();
                
            })
            .Accepts<Api.BookListEdit>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<ErrorMessage>(StatusCodes.Status500InternalServerError)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .WithTags("Book List")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Updates an entry for a given bookList ID."
            });

            //deletes a book from a user's booklist, but does not delete it from the main book database
            //requires id in URL and sessionKey in body
            app.MapDelete("/api/Booklist/{id}/delete", async (String id, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<Api.BookListRequestBody>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.missing_request_body, "booklist_delete", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == payload.SessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "booklist_delete", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                if (payload.SessionKey == currentSession.Session) {
                    DB.deleteFromBookList(id);
                    JsonLog.writeLog($"BookList ID {id} deleted from Booklist.", "INFO", "booklist_delete", currentSession, remoteIp);
                    return Results.Ok();
                }
                return Results.BadRequest();
            })
            .Accepts<Api.BookListRequestBody>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<ErrorMessage>(StatusCodes.Status500InternalServerError)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .WithTags("Book List")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Deletes a given bookList ID from the user's book list.",
                Description = "Currently the sessionKey is required in the request body, but it should move to an line parameter to be more consistent."
            });

            //retreives all of the data for a given booklist ID
            //requires id in URL and sessionKey in body
            app.MapGet("/api/Booklist/{id}/data", (String id, String sessionKey, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "booklist_dataRetrieval", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                DB.BookPageInfo data = DB.getBookPageData(id);
                return Results.Ok(data); 
            })
            .Produces<DB.BookPageInfo>(StatusCodes.Status200OK)
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .WithTags("Book List")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Gets all of the info for a given bookList ID."
            });

            app.MapPut("/api/Booklist/{id}/data", async (String id, String sessionKey, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "booklist_updateMetadata", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                if (requestBody == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.missing_request_body, "booklist_updateMetadata", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                BookListTypes.UpdateRequestBody updateData = JsonConvert.DeserializeObject<BookListTypes.UpdateRequestBody>(requestBody);
                if (updateData == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_request_body, "booklist_updateMetadata", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                JsonLog.writeLog($"Metadata for bookList Entry ID {id} has been updated.", "INFO", "booklist_updateMetadata", currentSession, remoteIp);
                DB.updateBookListMetadata(updateData);
                return Results.Ok();

            })
            .Accepts<BookListTypes.UpdateRequestBody>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .WithTags("Book List")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Updates the Google Books provided Metadata."
            });

        }

        public static class BookListTypes {

            public class UpdateRequestBody {
                public int? BookID { get; set; }
                public string? Title { get; set; }
                public string? Author { get; set; }
                public string? datePublished { get; set; }
                public string? Publisher { get; set; }
                public string? ImageLink { get; set; }
                public string? Description { get; set; }
                public int? PageCount { get; set; }
                public string? Isbn { get; set; }
                public string? Category { get; set; }
            }

        }
        
    }
}