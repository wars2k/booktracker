using Newtonsoft.Json;
using System.Text;

namespace bookTrackerApi {

    public static class StatsEndpoints {

        public static void configureEndpoints(WebApplication app) {

            app.MapGet("/api/statistics/statusCounts", (String sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest("Invalid session key");
                }
                StatsTypes.StatusCounts statusCounts = StatsDB.GetStatusCounts(currentSession);
                return Results.Ok(statusCounts);
            })
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<StatsTypes.StatusCounts>(StatusCodes.Status200OK)
            .WithTags("Statistics")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Retrieves the number of books assigned to each status for a given user."
            });

        }

    }

}