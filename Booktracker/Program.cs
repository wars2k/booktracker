namespace bookTrackerApi;

public static class Program
{
    public static List<SessionInfo> Sessions = new();

    public static void Main(string[] args)
    {
        new DB().InitiateDatabase();
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddCors();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.WebHost.ConfigureKestrel((hostingContext, options) =>
        {
            options.ListenAnyIP(5044); // Replace with the desired port
        });
        builder.Services.AddMvcCore().AddApiExplorer();

        WebApplication app = builder.Build();

        app.UseSwagger(c => { c.RouteTemplate = "api/docs/{documentName}/swagger.json"; });
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("v1/swagger.json", "Booktracker API");
            c.RoutePrefix = "api/docs";
        });
        app.UseDefaultFiles();
        app.UseStaticFiles();

        Api.Configure(app);
        Api.ConfigureBookEndpoints(app);
        BookListEndpoints.configure(app);
        AuthEndpoints.configure(app);
        UserEndpoints.configure(app);
        SettingsEndpoints.configure(app);
        CollectionEndpoints.configureCollectionEndpoints(app);
        StatsEndpoints.configureEndpoints(app);

        app.Run();
    }
}

public class SessionInfo
{
    public string? Session { get; set; }
    public string? AssociatedID { get; set; }
    public string? Username { get; set; }
    public int? IsAdmin { get; set; }
}