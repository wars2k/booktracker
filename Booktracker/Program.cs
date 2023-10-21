//using Microsoft.AspNetCore.OpenApi;

namespace bookTrackerApi {

    public static class Program {

        public static List<SessionInfo> Sessions = new List<SessionInfo>();

        public static List<ChallengeTypes.LocalChallenge> ActiveChallenges = new List<ChallengeTypes.LocalChallenge>();

        
        public static void Main(string[] args) {
            DB.InitiateDatabase();
            DB.initiateConnection();
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.WebHost.ConfigureKestrel((hostingContext, options) =>
            {
            options.ListenAnyIP(5000); // Replace with the desired port
            });
            builder.Services.AddMvcCore().AddApiExplorer();
            var app = builder.Build();

            app.UseSwagger(c => {
                c.RouteTemplate = "api/docs/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Booktracker API");
                c.RoutePrefix = "api/docs";
            });
            app.UseDefaultFiles();
            app.UseStaticFiles();

            Api.configure(app);
            Api.configureBookEndpoints(app);
            BookListEndpoints.configure(app);
            AuthEndpoints.configure(app);
            UserEndpoints.configure(app);
            SettingsEndpoints.configure(app);
            CollectionEndpoints.configureCollectionEndpoints(app);
            StatsEndpoints.configureEndpoints(app);
            JournalEndpoints.Configure(app);
            ChallengeEndpoints.configure(app);
            EventEndpoints.configure(app);

            ChallengeDB.storeChallenges();

            app.Run();
        }

        public static string loggingLevel = SettingsDB.getLoggingLevel();
        
    }

    public class SessionInfo {
        public string? Session { get; set; }
        public string? AssociatedID {get; set; }
        public string? Username {get; set; }
        public int? IsAdmin { get; set; }
    }


    
}
