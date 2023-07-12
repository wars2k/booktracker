using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace bookTrackerApi;

public static class AuthEndpoints
{
    public static void configure(WebApplication app)
    {
        //receives a username and password. Checks that with the database of users.
        //if both are correct, starts a new session and returns the session key to the client.
        app.MapPost("/api/login", async (HttpContext context) =>
            {
                using StreamReader reader = new StreamReader(context.Request.Body);
                string requestBody = await reader.ReadToEndAsync();
                Api.UserInfo? payload = JsonConvert.DeserializeObject<Api.UserInfo>(requestBody);
                if (payload == null)
                {
                    return Results.BadRequest();
                }

                if (payload.Username == null || payload.Password == null)
                {
                    return Results.BadRequest();
                }

                DB.UserInfo userInfo = new DB().retrieveUserInfo(payload.Username);
                if (userInfo.Password == payload.Password)
                {
                    string generateSession = Api.GenerateSessionKey(32);
                    SessionInfo newSession = new SessionInfo();
                    newSession.Session = generateSession;
                    newSession.AssociatedID = userInfo.Id;
                    newSession.Username = userInfo.Username;
                    newSession.IsAdmin = userInfo.IsAdmin;
                    Program.Sessions.Add(newSession);
                    Log.logSuccessfulLoginAttempt(payload.Username);
                    return Results.Ok(generateSession);
                }

                Log.logFailedLoginAttempt(payload.Username);
                return Results.Unauthorized();
            })
            .Accepts<Api.UserInfo>("application/json")
            .Produces<string>()
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .WithTags("Authorization/Registration")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Logs in a user and returns a sessionKey to be used in future requests",
                Description =
                    "If the credentials are correct, the server generates a sessionKey, stores it as an active session, and returns the sessionKey to the user to be used in future requests."
            });

        //logs out the user on the server-side by nullifying the sessionKey & associatedID
        app.MapPost("/api/logout", (string sessionKey) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession != null)
                {
                    Log.logSuccessfulLogout(currentSession);
                    Program.Sessions.Remove(currentSession);
                }

                return Results.Ok();
            })
            .Produces<string>()
            .WithTags("Authorization/Registration")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Ends the user's session on the backend."
            });

        app.MapGet("/api/register/canRegister", () =>
            {
                bool adminExists = new DB().checkForAdminUser();
                if (adminExists)
                {
                    return Results.Unauthorized();
                }

                return Results.Ok();
            })
            .Produces<string>()
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .WithTags("Authorization/Registration")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Checks to see if a user can register on the site.",
                Description =
                    "This endpoint checks to see if an admin user exists. If not, then it returns 200, therefore allowing the first user to register on the site as an admin."
            });

        app.MapPost("/api/register", async (HttpContext context) =>
            {
                using StreamReader reader = new StreamReader(context.Request.Body);
                string requestBody = await reader.ReadToEndAsync();
                Api.RegisterInfo? payload = JsonConvert.DeserializeObject<Api.RegisterInfo>(requestBody);
                if (payload == null)
                {
                    return Results.BadRequest();
                }

                new DB().registerUser(payload);
                return Results.Ok();
            })
            .Accepts<Api.RegisterInfo>("application/json")
            .Produces<string>()
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Authorization/Registration")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Registers an admin user on the site."
            });
    }
}