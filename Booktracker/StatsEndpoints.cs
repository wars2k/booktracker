using Microsoft.OpenApi.Models;

namespace bookTrackerApi;

public static class StatsEndpoints
{
    public static void configureEndpoints(WebApplication app)
    {
        app.MapGet("/api/statistics/statusCounts", (string sessionKey) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                {
                    return Results.BadRequest("Invalid session key");
                }

                StatsTypes.StatusCounts statusCounts = new StatsDB().GetStatusCounts(currentSession);
                return Results.Ok(statusCounts);
            })
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<StatsTypes.StatusCounts>()
            .WithTags("Statistics")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Retrieves the number of books assigned to each status for a given user."
            });
    }
}