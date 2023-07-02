//using Microsoft.AspNetCore.OpenApi;

namespace bookTrackerApi {

    public static class Program {

        public static List<SessionInfo> Sessions = new List<SessionInfo>();
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

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API Title V1");
                c.RoutePrefix = string.Empty; // Set the Swagger UI at the root URL
            });

            Api.configure(app);
            Api.configureBookEndpoints(app);
            CollectionEndpoints.configureCollectionEndpoints(app);
            StatsEndpoints.configureEndpoints(app);

            app.Run();
        }
    }

    public class SessionInfo {
        public string? Session { get; set; }
        public string? AssociatedID {get; set; }
        public string? Username {get; set; }
        public int? IsAdmin { get; set; }
    }


    
}
