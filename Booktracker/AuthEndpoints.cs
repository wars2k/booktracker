using Newtonsoft.Json;
using System.Text;

namespace bookTrackerApi {

    public static class AuthEndpoints {

        public static void configure(WebApplication app) {

            //receives a username and password. Checks that with the database of users.
            //if both are correct, starts a new session and returns the session key to the client.
            app.MapPost("/api/login", async (HttpContext context) => {

                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<Api.UserInfo>(requestBody);
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();

                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.missing_request_body, "login", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }

                if (payload.Username == null || payload.Password == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_request_body, "login", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }

                DB.UserInfo userInfo = DB.retrieveUserInfo(payload.Username);
                if (userInfo.Password == payload.Password) {
                    string generateSession = Api.generateSessionKey(32);
                    SessionInfo newSession  = new SessionInfo();
                    newSession.Session = generateSession;
                    newSession.AssociatedID = userInfo.Id;
                    newSession.Username = userInfo.Username;
                    newSession.IsAdmin = userInfo.IsAdmin;
                    Program.Sessions.Add(newSession);
                    JsonLog.writeLog("Successful login", "INFO", "login", newSession, remoteIp);
                    return Results.Ok(generateSession);
                } else {
                    JsonLog.writeLog("Login attempt failed due to invalid credentials.", "WARNING", "login", null, remoteIp);
                    return Results.Unauthorized();
                }   
            })
            .Accepts<Api.UserInfo>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .WithTags("Authorization/Registration")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Logs in a user and returns a sessionKey to be used in future requests",
                Description = "If the credentials are correct, the server generates a sessionKey, stores it as an active session, and returns the sessionKey to the user to be used in future requests."
            });

            //logs out the user on the server-side by nullifying the sessionKey & associatedID
            app.MapPost("/api/logout", (string sessionKey, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession != null) {
                    JsonLog.writeLog("Successful logout", "INFO", "logout", currentSession, remoteIp);
                    Log.logSuccessfulLogout(currentSession);
                    Program.Sessions.Remove(currentSession);
                }
                return Results.Ok();
            })
            .Produces<string>(StatusCodes.Status200OK)
            .WithTags("Authorization/Registration")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Ends the user's session on the backend."
            });

            app.MapGet("/api/register/canRegister", () => {
                Boolean adminExists = DB.checkForAdminUser();
                if (adminExists) {
                    return Results.Unauthorized();
                } else {
                    return Results.Ok();
                }
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .WithTags("Authorization/Registration")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Checks to see if a user can register on the site.",
                Description = "This endpoint checks to see if an admin user exists. If not, then it returns 200, therefore allowing the first user to register on the site as an admin."
            });

            app.MapPost("/api/register", async (HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<Api.RegisterInfo>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.missing_request_body, "registration", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                JsonLog.writeLog($"Initial admin account created. Username: '{payload.Username}'", "INFO", "registration", null, remoteIp);
                DB.registerUser(payload);
                return Results.Ok();
            })
            .Accepts<Api.RegisterInfo>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Authorization/Registration")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Registers an admin user on the site."
            });

            app.MapGet("/api/checkSession", (string sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.Unauthorized();
                } else {
                    return Results.Ok();
                }
            });

        }

    }

}