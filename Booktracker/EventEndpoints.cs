using Newtonsoft.Json;
using System.Text;

namespace bookTrackerApi {

    public static class EventEndpoints {

        public static void configure(WebApplication app) {

            app.MapGet("/api/BookList/{id}/events", (string id, string sessionKey, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "event_view", null, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                List<EventTypes.External> events = EventDB.GetEvents(Int32.Parse(id));
                return Results.Ok(events);
            });

        }

    }

}