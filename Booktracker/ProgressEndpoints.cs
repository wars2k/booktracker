using Newtonsoft.Json;
using System.Text;

namespace bookTrackerApi {

    public static class ProgressEndpoints {

        public static void configure(WebApplication app) {

            app.MapPost("/api/BookList/{id}/progress", async (int id, string sessionKey, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);

                //Check if sessionKey is valid.
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "progress_create", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }

                //deserialize and validate the request body.
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<ProgressTypes.RequestBody>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.missing_request_body, "progress_create", currentSession, remoteIp);
                    return Results.BadRequest(errorMessage);
                }

                //Create an Internal progress object and store it in the database
                ProgressTypes.Internal progressInfo = new(payload, id, Int32.Parse(currentSession.AssociatedID));
                int progressID = ProgressDB.Create(progressInfo);

                //Create a progress event and store it in the event table in the database.
                EventTypes.Internal progressEvent = new(Int32.Parse(currentSession.AssociatedID), id, EventTypes.EventCategories.progress, progressID.ToString());
                EventDB.Add(progressEvent);

                return Results.Ok(progressID);
            })
            .Accepts<ProgressTypes.RequestBody>("application/json")
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .WithTags("Progress")
            .WithOpenApi(operation => new(operation) {
                Summary = "Creates a new progress event for a given bookList ID."
            });

            app.MapGet("/api/BookList/{id}/progress", (int id, string sessionKey, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "progress_get", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                List<ProgressTypes.ExternalProg> progressList = ProgressDB.GetAll(id);
                return Results.Ok(progressList);
            })
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces<List<ProgressTypes.ExternalProg>>(StatusCodes.Status200OK)
            .WithTags("Progress")
            .WithOpenApi(operation => new(operation) {
                Summary = "Retrieves all progress events for a given bookList ID."
            });

            app.MapGet("/api/BookList/{id}/progress/{progressID}", (int id, int progressID, string sessionKey, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "progress_getOne", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                ProgressTypes.ExternalProg progress = ProgressDB.GetOne(progressID);
                return Results.Ok(progress);
            })
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces<ProgressTypes.ExternalProg>(StatusCodes.Status200OK)
            .WithTags("Progress")
            .WithOpenApi(operation => new(operation) {
                Summary = "Retrieves a specific progress event by ID for a given bookList ID."
            });

        }
    
    }

}