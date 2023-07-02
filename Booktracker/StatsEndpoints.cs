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
            });

        }

    }

}