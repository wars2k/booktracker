using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace bookTrackerApi;

public static class BookListEndpoints
{
    public static void configure(WebApplication app)
    {
        //retrieves a user's bookList based on user ID if provided sessionkey is correct
        app.MapPut("/api/getBookList", async (HttpContext context) =>
            {
                using StreamReader reader = new StreamReader(context.Request.Body);
                string requestBody = await reader.ReadToEndAsync();
                Api.BookListRequestBody? payload = JsonConvert.DeserializeObject<Api.BookListRequestBody>(requestBody);
                if (payload == null)
                {
                    Log.AlertFailedBookListRetrieval("emptyPayload", null);
                    return Results.BadRequest("emptyPayload");
                }

                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == payload.SessionKey);
                if (currentSession == null)
                {
                    Log.AlertFailedBookListRetrieval("noBackEndSession", null);
                    return Results.BadRequest("noBackEndSession");
                }

                if (payload.SessionKey == currentSession.Session)
                {
                    List<DB.BookListInfo> bookList = new DB().getBookListForUser(currentSession.AssociatedID);
                    return Results.Ok(bookList);
                }

                Log.AlertFailedBookListRetrieval("incorrectSessionKey", null);
                return Results.Unauthorized();
            })
            .Accepts<Api.BookListRequestBody>("application/json")
            .Produces<List<DB.BookListInfo>>()
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .WithTags("Book List")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Retrieves a list of all of a given user's books.",
                Description = "Currently this endpoint is a PUT when it should be a GET."
            });


        //updates an entry for a given userID's booklist
        app.MapPut("/api/BookList/{id}", async (HttpContext context) =>
            {
                using StreamReader reader = new StreamReader(context.Request.Body);
                string requestBody = await reader.ReadToEndAsync();
                Api.BookListEdit? payload = JsonConvert.DeserializeObject<Api.BookListEdit>(requestBody);
                if (payload == null)
                {
                    Log.AlertFailedBookListEdit("emptyPayload", null);
                    return Results.BadRequest();
                }

                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == payload.SessionKey);
                if (currentSession == null)
                {
                    Log.AlertFailedBookListEdit("noBackEndSession", null);
                    return Results.BadRequest();
                }

                if (payload.SessionKey == currentSession.Session && payload.Data != null)
                {
                    new DB().updateBookList(payload.Data);
                    Log.AlertSuccessfulBookListEdit(payload.Data, currentSession);
                    return Results.Ok();
                }

                if (payload.SessionKey != currentSession.AssociatedID)
                {
                    Log.AlertFailedBookListEdit("incorrectSessionKey", currentSession);
                    return Results.Unauthorized();
                }

                Log.AlertFailedBookListEdit("unknownError", currentSession);
                return Results.BadRequest();
            })
            .Accepts<Api.BookListEdit>("application/json")
            .Produces<string>()
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .WithTags("Book List")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Updates an entry for a given bookList ID."
            });

        //deletes a book from a user's booklist, but does not delete it from the main book database
        //requires id in URL and sessionKey in body
        app.MapDelete("/api/Booklist/{id}/delete", async (string id, HttpContext context) =>
            {
                using StreamReader reader = new StreamReader(context.Request.Body);
                string requestBody = await reader.ReadToEndAsync();
                Api.BookListRequestBody? payload = JsonConvert.DeserializeObject<Api.BookListRequestBody>(requestBody);
                if (payload == null)
                {
                    Log.AlertFailedBookListDelete("emptyPayload", null);
                    return Results.BadRequest();
                }

                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == payload.SessionKey);
                if (currentSession == null)
                {
                    Log.AlertFailedBookListDelete("noBackEndSession", null);
                    return Results.BadRequest();
                }

                if (payload.SessionKey == currentSession.Session)
                {
                    new DB().deleteFromBookList(id);
                    Log.AlertSuccessfulBookListDelete(id, currentSession);
                    return Results.Ok();
                }

                if (payload.SessionKey != currentSession.AssociatedID)
                {
                    Log.AlertFailedBookListDelete("incorrectSessionKey", currentSession);
                    return Results.Unauthorized();
                }

                Log.AlertFailedBookListDelete("unknownError", currentSession);
                return Results.BadRequest();
            })
            .Accepts<Api.BookListRequestBody>("application/json")
            .Produces<string>()
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .WithTags("Book List")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Deletes a given bookList ID from the user's book list.",
                Description =
                    "Currently the sessionKey is required in the request body, but it should move to an line parameter to be more consistent."
            });

        //retreives all of the data for a given booklist ID
        //requires id in URL and sessionKey in body
        app.MapGet("/api/Booklist/{id}/data", (string id, string sessionKey) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                {
                    return Results.BadRequest();
                }

                DB.BookPageInfo data = new DB().getBookPageData(id);
                return Results.Ok(data);
            })
            .Produces<DB.BookPageInfo>()
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Book List")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Gets all of the info for a given bookList ID."
            });

        app.MapPut("/api/Booklist/{id}/data", async (string id, string sessionKey, HttpContext context) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                {
                    return Results.BadRequest();
                }

                using StreamReader reader = new StreamReader(context.Request.Body);
                string? requestBody = await reader.ReadToEndAsync();
                if (requestBody == null)
                {
                    return Results.BadRequest();
                }

                BookListTypes.UpdateRequestBody? updateData = JsonConvert.DeserializeObject<BookListTypes.UpdateRequestBody>(requestBody);
                if (updateData == null)
                {
                    return Results.BadRequest("incorrect format for body");
                }

                new DB().updateBookListMetadata(updateData);
                return Results.Ok();
            })
            .Accepts<BookListTypes.UpdateRequestBody>("application/json")
            .Produces<string>()
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Book List")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Updates the Google Books provided Metadata."
            });
    }

    public static class BookListTypes
    {
        public class UpdateRequestBody
        {
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