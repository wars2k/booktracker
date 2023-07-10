using Newtonsoft.Json;
using System.Text;

namespace bookTrackerApi {

    public static class SettingsEndpoints {

        public static void configure(WebApplication app) {

            app.MapGet("/api/data/export", (String format, String sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest();
                }
                List<DB.BookPageInfo> ListOfBookData = DB.getBookDataForExport(int.Parse(currentSession.AssociatedID));
                if (format == "json") {
                    Export.ExportDataAsJSON(ListOfBookData, currentSession.Username);
                } else if (format == "csv") {
                    Export.ExportDataAsCSV(ListOfBookData, currentSession.Username);
                } else {
                    return Results.BadRequest();
                }
                byte[] test = File.ReadAllBytes($"external/export/{currentSession.Username}-export.{format}");
                return Results.File(test, "text/csv", $"bookExport.{format}");
            })
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<FileStream>(StatusCodes.Status200OK)
            .WithTags("Settings")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Returns a file for download with all of a user's data",
                Description = "Format depends on the 'format' inline parameter. The options are 'csv' and 'json'."
            });



            app.MapPost("/api/data/import", async (String format, String sessionKey, IFormFile file) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest();
                }
                if (format == "goodreads") {
                    await Import.ImportFromGoodreads(file, currentSession);
                }
                return Results.Ok();
                //read the file
            })
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status200OK)
            .WithTags("Settings")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Imports data from an external source.",
                Description = "Format depends on the 'format' inline parameter. The options are 'goodreads'."
            });

            

            app.MapGet("/api/test/test", () => {
                return Results.Ok("test");
            })
            .Produces<string>(StatusCodes.Status200OK)
            .WithTags("Settings");



            app.MapPut("/api/settings", (String results, String sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest();
                }
                ApiClient.Results = int.Parse(results);
                return Results.Ok();
            })
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status200OK)
            .WithTags("Settings")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Changes settings globally for all users."
            });

        }

    }

}