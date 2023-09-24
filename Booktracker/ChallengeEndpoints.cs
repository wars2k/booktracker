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
                return Results.Ok();
            });

            app.MapDelete("/api/challenges/{challengeID}", async (HttpContext context, string sessionKey, string challengeID) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "challenges_create", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                ChallengeDB.delete(challengeID);
                return Results.Ok();
            });

        }
    
    }

}