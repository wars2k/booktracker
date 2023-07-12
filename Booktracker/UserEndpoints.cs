using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace bookTrackerApi;

public static class UserEndpoints
{
    public static void configure(WebApplication app)
    {
        app.MapPost("/api/users/new", async (string sessionKey, HttpContext context) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                    //log failed attempt to create new user
                {
                    return Results.BadRequest();
                }

                if (currentSession.IsAdmin == 0)
                    //log unauthorized user attempted to add new user
                {
                    return Results.Unauthorized();
                }

                using StreamReader reader = new StreamReader(context.Request.Body);
                string requestBody = await reader.ReadToEndAsync();
                Api.NewUserPayload? payload = JsonConvert.DeserializeObject<Api.NewUserPayload>(requestBody);
                if (payload == null)
                {
                    return Results.BadRequest();
                }

                if (payload.Username == null || payload.Password == null)
                {
                    return Results.BadRequest("Must include new username & password in request body");
                }

                new DB().createNewUser(payload);
                Log.writeLog($"New user ({payload.Username}) created by admin user {currentSession.Username}.",
                    "INFO");
                return Results.Ok();
            })
            .Accepts<Api.NewUserPayload>("application/json")
            .Produces<string>()
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("User Management")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Creates a new user."
            });

        app.MapGet("/api/users", (string sessionKey) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                    //log failed attempt to access user list
                {
                    return Results.BadRequest();
                }

                if (currentSession.IsAdmin == 0)
                    //log unauthorized user attempted to access user list
                {
                    return Results.Unauthorized();
                }

                List<Api.UserData> userData = new DB().getUserData();
                return Results.Ok(userData);
            })
            .Produces<List<Api.UserData>>()
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags("User Management")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Retrieves a list of all users."
            });

        app.MapPut("/api/users/{id}", async (string id, string sessionKey, HttpContext context) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                    //log failed attempt to create new user
                {
                    return Results.BadRequest();
                }

                if (currentSession.IsAdmin == 0)
                    //log unauthorized user attempted to add new user
                {
                    return Results.Unauthorized();
                }

                using StreamReader reader = new StreamReader(context.Request.Body);
                string requestBody = await reader.ReadToEndAsync();
                Api.UserData? payload = JsonConvert.DeserializeObject<Api.UserData>(requestBody);
                if (payload == null)
                {
                    return Results.BadRequest();
                }

                new DB().updateUser(int.Parse(id), payload);
                return Results.Ok();
            })
            .Accepts<Api.UserData>("application/json")
            .Produces<string>()
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("User Management")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Updates a user's info."
            });

        app.MapDelete("/api/users/{id}", (string id, string sessionKey) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                {
                    return Results.BadRequest();
                }

                if (currentSession.IsAdmin == 0)
                {
                    return Results.Unauthorized();
                }

                new DB().deleteUser(int.Parse(id));
                return Results.Ok();
            })
            .Produces<string>()
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("User Management")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Deletes a user."
            });
    }
}