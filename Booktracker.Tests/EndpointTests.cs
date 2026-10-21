using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using bookTrackerApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class EndpointTests
{
    public static Task Search429() => ProviderFailure("GET", "/api/books/new", HttpStatusCode.TooManyRequests);
    public static Task Search503() => ProviderFailure("GET", "/api/books/new", HttpStatusCode.Forbidden);
    public static Task Save429() => ProviderFailure("POST", "/api/books/save", HttpStatusCode.TooManyRequests);
    public static Task Save503() => ProviderFailure("POST", "/api/books/save", HttpStatusCode.BadGateway);
    public static Task OversizedDelta429() => OversizedRetryAfter(false);
    public static Task OversizedDate429() => OversizedRetryAfter(true);

    private static async Task ProviderFailure(string method, string path, HttpStatusCode providerStatus)
    {
        await using var fixture = new Fixture(providerStatus);
        var before = fixture.CountWrites();
        var response = await fixture.Invoke(method, path);
        var rateLimited = providerStatus == HttpStatusCode.TooManyRequests;

        AssertSafeError(response, rateLimited);
        Require(response.RetryAfter == (rateLimited ? "7" : null), "Incorrect Retry-After response header.");
        Require(fixture.RequestCount == (rateLimited ? 3 : 1), "The endpoint did not use the fake provider as expected.");
        Require(fixture.CountWrites() == before, "Provider failure wrote books, user_books or book_events.");
        Require(!File.ReadAllText("external/log/log.txt").Contains(Fixture.PrivateMarker), "Provider details leaked to logs.");
    }

    private static async Task OversizedRetryAfter(bool useDate)
    {
        await using var fixture = new Fixture(HttpStatusCode.TooManyRequests, useDate);
        var before = fixture.CountWrites();
        var response = await fixture.Invoke("POST", "/api/books/save");

        AssertSafeError(response, true);
        Require(response.RetryAfter == "5184000", "Unsupported retry timing was lost or shortened.");
        Require(fixture.RequestCount == 1, "Unsupported Retry-After caused another provider request.");
        Require(fixture.CountWrites() == before, "Rate-limit failure wrote application data.");
    }

    public static async Task InvalidSessionsDoNotQueryProvider()
    {
        await using var fixture = new Fixture(HttpStatusCode.ServiceUnavailable);
        var before = fixture.CountWrites();
        foreach (var (method, path) in new[] { ("GET", "/api/books/new"), ("POST", "/api/books/save") })
        {
            var response = await fixture.Invoke(method, path, validSession: false);
            Require(response.Status == 400, "Existing invalid-session behavior changed.");
            using var body = JsonDocument.Parse(response.Body);
            Require(body.RootElement.GetProperty("code").GetString() == "invalid_sessionKey", "Missing session error.");
        }
        Require(fixture.RequestCount == 0, "Invalid session invoked the provider.");
        Require(fixture.CountWrites() == before, "Invalid session wrote application data.");
    }

    public static async Task ManualEntryDoesNotQueryProvider()
    {
        await using var fixture = new Fixture(HttpStatusCode.ServiceUnavailable);
        var response = await fixture.Invoke("POST", "/api/books/new/manual", payload: new {
            title = "Offline fixture book", author = "Fixture author", publisher = "Fixture publisher", date = "2026", image = ""
        });
        Require(response.Status == 200, "Manual Entry failed with an unavailable provider.");
        Require(fixture.RequestCount == 0, "Manual Entry invoked the provider.");
        Require(fixture.CountWrites() == (1, 1, 0), "Manual Entry did not persist its book and list entry.");
    }

    private static void AssertSafeError(Result response, bool rateLimited)
    {
        Require(response.Status == (rateLimited ? 429 : 503), "Incorrect API failure status.");
        Require(response.ContentType?.StartsWith("application/json") == true, "Error response is not JSON.");
        using var body = JsonDocument.Parse(response.Body);
        var root = body.RootElement;
        Require(root.EnumerateObject().Count() == 3, "Unexpected fields in API error response.");
        Require(root.GetProperty("code").GetString() == (rateLimited ? "google_books_rate_limited" : "google_books_unavailable"), "Incorrect error code.");
        Require(root.GetProperty("message").GetString() == (rateLimited ? "Google Books is temporarily rate limited." : "Google Books is temporarily unavailable."), "Incorrect user message.");
        Require(root.GetProperty("details").GetString() == "Try again later or use Manual Entry.", "Manual Entry guidance is missing.");
        Require(!response.Body.Contains(Fixture.PrivateMarker) && !response.Body.Contains("googleapis.com") && !response.Body.Contains("Exception"), "Internal provider details leaked to response.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed record Result(int Status, string Body, string? ContentType, string? RetryAfter);

    // The console runner executes these sequentially because legacy DB/log paths are process-relative.
    private sealed class Fixture : IAsyncDisposable
    {
        public const string PrivateMarker = "private-provider-fixture";
        private readonly string _previousDirectory = Directory.GetCurrentDirectory();
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "booktracker-endpoint-tests-" + Guid.NewGuid().ToString("N"));
        private readonly QueueHttpMessageHandler _handler;
        private readonly HttpClient _http;
        private readonly WebApplication _app;
        private readonly SessionInfo _session;

        public Fixture(HttpStatusCode providerStatus, bool? oversizedDate = null)
        {
            var now = new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);
            Directory.CreateDirectory(Path.Combine(_directory, "external", "db"));
            Directory.CreateDirectory(Path.Combine(_directory, "external", "log"));
            File.Copy(Path.Combine(AppContext.BaseDirectory, "init.sql"), Path.Combine(_directory, "init.sql"));
            Directory.SetCurrentDirectory(_directory);
            try
            {
                DB.InitiateDatabase();
                using (var connection = DB.initiateConnection())
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO users (idusers, username, admin) VALUES (1, 'endpoint-fixture', 1)";
                    command.ExecuteNonQuery();
                }
                _session = new SessionInfo { Session = "endpoint-fixture-session", AssociatedID = "1", Username = "endpoint-fixture", IsAdmin = 1 };
                bookTrackerApi.Program.Sessions.Add(_session);
                Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond = (_, _) => {
                    var response = new HttpResponseMessage(providerStatus) { Content = new StringContent(PrivateMarker) };
                    if (providerStatus == HttpStatusCode.TooManyRequests)
                    {
                        response.Headers.RetryAfter = oversizedDate switch {
                            true => new RetryConditionHeaderValue(now.AddDays(60)),
                            false => new RetryConditionHeaderValue(TimeSpan.FromDays(60)),
                            null => new RetryConditionHeaderValue(TimeSpan.FromSeconds(7))
                        };
                    }
                    return Task.FromResult(response);
                };
                _handler = new QueueHttpMessageHandler(Enumerable.Repeat(respond, 3).ToArray());
                _http = new HttpClient(_handler);
                var provider = new ApiClient(PrivateMarker, _http, (_, _) => Task.CompletedTask, () => now);
                var builder = WebApplication.CreateBuilder(new WebApplicationOptions {
                    ApplicationName = typeof(Api).Assembly.GetName().Name,
                    ContentRootPath = _directory,
                    EnvironmentName = "Production"
                });
                builder.Logging.ClearProviders();
                builder.Services.AddSingleton(provider);
                builder.Services.AddEndpointsApiExplorer();
                _app = builder.Build();
                Api.configureBookEndpoints(_app);
            }
            catch
            {
                Directory.SetCurrentDirectory(_previousDirectory);
                SqliteConnection.ClearAllPools();
                Directory.Delete(_directory, true);
                throw;
            }
        }

        public int RequestCount => _handler.RequestUris.Count;

        public (long Books, long UserBooks, long Events) CountWrites()
        {
            using var connection = DB.initiateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT (SELECT COUNT(*) FROM books), (SELECT COUNT(*) FROM user_books), (SELECT COUNT(*) FROM book_events)";
            using var reader = command.ExecuteReader();
            reader.Read();
            return (reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2));
        }

        public async Task<Result> Invoke(string method, string path, bool validSession = true, object? payload = null)
        {
            var endpoint = ((IEndpointRouteBuilder)_app).DataSources.SelectMany(source => source.Endpoints)
                .OfType<RouteEndpoint>().Single(endpoint => endpoint.RoutePattern.RawText == path
                    && endpoint.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods.Contains(method));
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var responseBody = new MemoryStream();
            using var requestBody = new MemoryStream(payload == null ? Array.Empty<byte>() : JsonSerializer.SerializeToUtf8Bytes(payload));
            var context = new DefaultHttpContext { RequestServices = _app.Services, RequestAborted = timeout.Token };
            context.Request.Method = method;
            context.Request.Path = path;
            context.Request.QueryString = QueryString.Create(new Dictionary<string, string?> {
                ["name"] = "fixture-query", ["results"] = "1", ["id"] = "fixture-volume",
                ["sessionKey"] = validSession ? _session.Session : "invalid-fixture-session"
            });
            context.Request.Body = requestBody;
            context.Request.ContentType = "application/json";
            context.Response.Body = responseBody;
            await endpoint.RequestDelegate!(context).WaitAsync(TimeSpan.FromSeconds(4));
            return new Result(context.Response.StatusCode, Encoding.UTF8.GetString(responseBody.ToArray()),
                context.Response.ContentType, context.Response.Headers.TryGetValue("Retry-After", out var retryAfter) ? retryAfter.ToString() : null);
        }

        public async ValueTask DisposeAsync()
        {
            bookTrackerApi.Program.Sessions.Remove(_session);
            await _app.DisposeAsync();
            _http.Dispose();
            SqliteConnection.ClearAllPools();
            Directory.SetCurrentDirectory(_previousDirectory);
            Directory.Delete(_directory, true);
        }
    }
}
