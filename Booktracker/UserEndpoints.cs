using Newtonsoft.Json;
using System.Text;

namespace bookTrackerApi {

    public static class UserEndpoints {

        public static void configure(WebApplication app) {

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
                var payload = JsonConvert.DeserializeObject<Api.NewUserPayload>(requestBody);
                if (payload == null) {
                    return Results.BadRequest();
                }
                if (payload.Username == null || payload.Password == null) {
                    return Results.BadRequest("Must include new username & password in request body");
                }
                DB.createNewUser(payload);
                Log.writeLog($"New user ({payload.Username}) created by admin user {currentSession.Username}.", "INFO");
                return Results.Ok();
            })
            .Accepts<Api.NewUserPayload>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("User Management")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Creates a new user."
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
                List<Api.UserData> userData = DB.getUserData();
                return Results.Ok(userData);
            })
            .Produces<List<Api.UserData>>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags("User Management")
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
                var payload = JsonConvert.DeserializeObject<Api.UserData>(requestBody);
                if (payload == null) {
                    return Results.BadRequest();
                }
                DB.updateUser(int.Parse(id), payload);
                return Results.Ok();
            })
            .Accepts<Api.UserData>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("User Management")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Updates a user's info."
            });

            app.MapDelete("/api/users/{id}", (String id, String sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest();
                }
                if (currentSession.IsAdmin == 0) {
                    return Results.Unauthorized();
                }
                DB.deleteUser(int.Parse(id));
                return Results.Ok();
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("User Management")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Deletes a user."
            });

        }

    }

}