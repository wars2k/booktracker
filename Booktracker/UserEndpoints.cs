using Newtonsoft.Json;
using System.Text;

namespace bookTrackerApi {

    public static class UserEndpoints {

        public static void configure(WebApplication app) {

            app.MapPost("/api/users/new", async (String sessionKey, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "user_create", null, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                if (currentSession.IsAdmin == 0) {
                    JsonLog.writeLog("Unauthorized attempt to create a new user.", "WARNING", "user_create", currentSession, remoteIp);
                    return Results.Unauthorized();
                }
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<Api.NewUserPayload>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_request_body, "user_create", currentSession, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                if (payload.Username == null || payload.Password == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_request_body, "user_create", currentSession, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                DB.createNewUser(payload);
                JsonLog.writeLog($"New user '{payload.Username}' created.", "INFO", "user_create", currentSession, remoteIp);
                return Results.Ok();
            })
            .Accepts<Api.NewUserPayload>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .WithTags("User Management")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Creates a new user."
            });

            app.MapGet("/api/users", (String sessionKey, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "user_database", null, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                if (currentSession.IsAdmin == 0) {
                    JsonLog.writeLog("Unauthorized attempt to access user database.", "WARNING", "user_database", currentSession, remoteIp);
                    return Results.Unauthorized();
                }
                List<Api.UserData> userData = DB.getUserData();
                return Results.Ok(userData);
            })
            .Produces<List<Api.UserData>>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .WithTags("User Management")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Retrieves a list of all users."
            });

            app.MapPut("/api/users/{id}", async (String id, String sessionKey, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "user_update", null, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                if (currentSession.IsAdmin == 0) {
                    JsonLog.writeLog("Unauthorized attempt to update a user's info.", "WARNING", "user_update", currentSession, remoteIp);
                    return Results.Unauthorized();
                }
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<Api.UserData>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_request_body, "user_update", currentSession, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                JsonLog.writeLog($"Info for User ID {id} has been updated.", "INFO", "user_update", currentSession, remoteIp);
                DB.updateUser(int.Parse(id), payload);
                return Results.Ok();
            })
            .Accepts<Api.UserData>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .WithTags("User Management")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Updates a user's info."
            });

            app.MapDelete("/api/users/{id}", (String id, String sessionKey, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "user_delete", null, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                if (currentSession.IsAdmin == 0) {
                    JsonLog.writeLog("Unauthorized attempt to delete a user.", "WARNING", "user_delete", currentSession, remoteIp);
                    return Results.Unauthorized();
                }
                JsonLog.writeLog($"User ID {id} deleted.", "INFO", "user_delete", currentSession, remoteIp);
                DB.deleteUser(int.Parse(id));
                return Results.Ok();
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .WithTags("User Management")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Deletes a user."
            });

        }

    }

}