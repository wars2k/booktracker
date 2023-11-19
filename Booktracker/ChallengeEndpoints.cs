using Newtonsoft.Json;
using System.Text;

namespace bookTrackerApi {

    public static class ChallengeEndpoints {

        public static void configure(WebApplication app) {

            app.MapGet("/api/challenges", async (HttpContext context, string sessionKey) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "challenges_view", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                List<ChallengeTypes.Challenge> challenges = ChallengeDB.getAll(currentSession);
                return Results.Ok(challenges);
            })
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces<List<ChallengeTypes.Challenge>>(StatusCodes.Status200OK)
            .WithTags("Challenges")
            .WithOpenApi(operation => new(operation) {
                Summary = "Gets all challenges for a specific user."
            });

            app.MapPost("/api/challenges", async (HttpContext context, string sessionKey) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "challenges_create", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<ChallengeTypes.NewChallenge>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.missing_request_body, "challenges_create", currentSession, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                ChallengeDB.create(payload, currentSession);
                ChallengeDB.storeChallenges();
                return Results.Ok();
            })
            .Accepts<ChallengeTypes.NewChallenge>("application/json")
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .WithTags("Challenges")
            .WithOpenApi(operation => new(operation) {
                Summary = "Creates a new user-specific challenge."
            });
            

            app.MapDelete("/api/challenges/{challengeID}", async (HttpContext context, string sessionKey, string challengeID) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "challenges_create", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                ChallengeDB.delete(challengeID);
                ChallengeDB.storeChallenges();
                return Results.Ok();
            })
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .WithTags("Challenges")
            .WithOpenApi(operation => new(operation) {
                Summary = "Deletes a specific challenge."
            });

        }
    
    }

}