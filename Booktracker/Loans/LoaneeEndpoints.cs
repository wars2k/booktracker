using Newtonsoft.Json;

namespace bookTrackerApi.Loans {

    public static class LoaneeEndpoints {

        public static void Configure(WebApplication app) {

            app.MapPost("/api/loans/loanees", async (HttpContext context, string sessionKey) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "loanee_create", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }

                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<Types.NewLoanee>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.missing_request_body, "loanee_create", currentSession, remoteIp);
                    return Results.BadRequest(errorMessage);
                }

                int loaneeID = LoaneeDB.AddLoanee(payload, Int32.Parse(currentSession.AssociatedID));
                return Results.Ok(loaneeID);

            })
            .Accepts<Types.NewLoanee>("application/json")
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces<int>(StatusCodes.Status200OK)
            .WithTags("Loans")
            .WithOpenApi(operation => new(operation) {
                Summary = "Creates a new loanee and returns the newly created ID."
            });

            app.MapGet("/api/loans/loanees", async (HttpContext context, string sessionKey) => {
               string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "loanee_get", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }

                List<Types.LoaneeName> loanees = LoaneeDB.GetNames(Int32.Parse(currentSession.AssociatedID));
                return Results.Ok(loanees);
            })
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces<int>(StatusCodes.Status200OK)
            .WithTags("Loans")
            .WithOpenApi(operation => new(operation) {
                Summary = "Returns all loanees (id, name) created by the requesting user."
            });

        }

    }

}