using Newtonsoft.Json;

namespace bookTrackerApi.Loans {

    public static class LoanEndpoints {

        public static void Configure(WebApplication app) {

            app.MapPost("/api/loans", async (HttpContext context, string sessionKey) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "loan_create", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }

                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<Types.NewLoan>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.missing_request_body, "loan_create", currentSession, remoteIp);
                    return Results.BadRequest(errorMessage);
                }

                int loanID = LoanDB.AddLoan(payload, Int32.Parse(currentSession.AssociatedID));
                return Results.Ok(loanID);

            })
            .Accepts<Types.NewLoan>("application/json")
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces<int>(StatusCodes.Status200OK)
            .WithTags("Loans")
            .WithOpenApi(operation => new(operation) {
                Summary = "Creates a new loan and returns the newly created ID."
            });

            app.MapGet("/api/loans", async (HttpContext context, string sessionKey, int? bookListID, string? status, int? loaneeID) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "loan_create", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }

                List<Types.BasicLoanInfo> loans = LoanDB.GetLoans(Int32.Parse(currentSession.AssociatedID), bookListID, status, loaneeID);
                return Results.Ok(loans);
            })
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .Produces<List<Types.BasicLoanInfo>>(StatusCodes.Status200OK)
            .WithTags("Loans")
            .WithOpenApi(operation => new(operation) {
                Summary = "Gets all loans for a given user. Can filter by status, bookListID, and loaneeID."
            });

        }

    }

}