using System.Net;
using System.Net.Http.Headers;
using System.Text;
using bookTrackerApi;

return await TestRunner.RunAsync();

internal static class TestRunner
{
    private const string EmptySearchJson = "{\"totalItems\":0}";
    private const string DetailJson = "{\"id\":\"volume-1\",\"volumeInfo\":{\"title\":\"Test Book\",\"authors\":[\"Test Author\"],\"pageCount\":\"123\"}}";

    public static async Task<int> RunAsync()
    {
        var tests = new (string Name, Func<Task> Run)[]
        {
            ("search omits blank key and encodes query", SearchOmitsBlankKeyAndEncodesQuery),
            ("search and detail include configured key", SearchAndDetailIncludeConfiguredKey),
            ("configured default preserves legacy static callers", ConfiguredDefaultSupportsLegacyStaticCallers),
            ("429 honors Retry-After delta", RetryAfterDeltaIsUsed),
            ("429 honors Retry-After date", RetryAfterDateIsUsed),
            ("invalid Retry-After uses exponential fallback", InvalidRetryAfterUsesFallback),
            ("429 uses exponential fallback and stops after three attempts", ExponentialFallbackIsBounded),
            ("non-429 status becomes safe provider failure", NonRateLimitStatusIsSafeFailure),
            ("transport failure becomes safe provider failure", TransportFailureIsSafe),
            ("malformed JSON becomes safe provider failure", MalformedJsonIsSafeFailure),
            ("caller cancellation is preserved", CallerCancellationIsPreserved),
        };

        var failures = 0;

        foreach (var test in tests)
        {
            try
            {
                await test.Run();
                Console.WriteLine($"PASS {test.Name}");
            }
            catch (Exception exception)
            {
                failures++;
                Console.WriteLine($"FAIL {test.Name}: {exception.GetType().Name}: {exception.Message}");
            }
        }

        Console.WriteLine($"{tests.Length - failures}/{tests.Length} checks passed.");
        return failures == 0 ? 0 : 1;
    }

    private static async Task SearchOmitsBlankKeyAndEncodesQuery()
    {
        var handler = new QueueHttpMessageHandler(Respond(HttpStatusCode.OK, EmptySearchJson));
        var client = CreateClient(handler, "   ");

        await client.CallApiAsync("Jane Austen", "12", CancellationToken.None);

        var uri = SingleRequest(handler);
        AssertContains(uri.AbsoluteUri, "q=Jane%20Austen");
        AssertContains(uri.AbsoluteUri, "maxResults=12");
        AssertFalse(uri.Query.Contains("key=", StringComparison.OrdinalIgnoreCase), "Blank API key must be omitted.");
    }

    private static async Task SearchAndDetailIncludeConfiguredKey()
    {
        const string apiKey = "test key";
        var handler = new QueueHttpMessageHandler(
            Respond(HttpStatusCode.OK, EmptySearchJson),
            Respond(HttpStatusCode.OK, DetailJson));
        var client = CreateClient(handler, apiKey);

        await client.CallApiAsync("C# basics", "5", CancellationToken.None);
        var book = await client.GetBookFromID("id/with space", CancellationToken.None);

        AssertEqual(2, handler.RequestUris.Count, "Expected search and detail requests.");
        AssertContains(handler.RequestUris[0].AbsoluteUri, "q=C%23%20basics");
        AssertContains(handler.RequestUris[0].AbsoluteUri, "key=test%20key");
        AssertContains(handler.RequestUris[1].AbsoluteUri, "/id%2Fwith%20space");
        AssertContains(handler.RequestUris[1].AbsoluteUri, "key=test%20key");
        AssertEqual("Test Book", book.Title, "Detail response should deserialize.");
    }

    private static async Task ConfiguredDefaultSupportsLegacyStaticCallers()
    {
        var handler = new QueueHttpMessageHandler(Respond(HttpStatusCode.OK, EmptySearchJson));
        var configuredClient = CreateClient(handler, "configured-key");
        ApiClient.ConfigureDefault(configuredClient);

        await ApiClient.CallApiAsync("legacy", "1");

        AssertContains(SingleRequest(handler).AbsoluteUri, "key=configured-key");
    }

    private static async Task RetryAfterDeltaIsUsed()
    {
        var delays = new List<TimeSpan>();
        var handler = new QueueHttpMessageHandler(
            Respond(HttpStatusCode.TooManyRequests, retryAfter: new RetryConditionHeaderValue(TimeSpan.FromSeconds(7))),
            Respond(HttpStatusCode.OK, EmptySearchJson));
        var client = CreateClient(handler, delays: delays);

        await client.CallApiAsync("retry", "1", CancellationToken.None);

        AssertEqual(2, handler.RequestUris.Count, "Expected one retry.");
        AssertSequence(delays, TimeSpan.FromSeconds(7));
    }

    private static async Task RetryAfterDateIsUsed()
    {
        var now = new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);
        var delays = new List<TimeSpan>();
        var handler = new QueueHttpMessageHandler(
            Respond(HttpStatusCode.TooManyRequests, retryAfter: new RetryConditionHeaderValue(now.AddSeconds(9))),
            Respond(HttpStatusCode.OK, EmptySearchJson));
        var client = CreateClient(handler, delays: delays, utcNow: now);

        await client.CallApiAsync("retry", "1", CancellationToken.None);

        AssertSequence(delays, TimeSpan.FromSeconds(9));
    }

    private static async Task InvalidRetryAfterUsesFallback()
    {
        var delays = new List<TimeSpan>();
        var handler = new QueueHttpMessageHandler(
            RespondWithInvalidRetryAfter(),
            Respond(HttpStatusCode.OK, EmptySearchJson));
        var client = CreateClient(handler, delays: delays);

        await client.CallApiAsync("retry", "1", CancellationToken.None);

        AssertSequence(delays, TimeSpan.FromSeconds(1));
    }

    private static async Task ExponentialFallbackIsBounded()
    {
        const string apiKey = "do-not-leak-this-key";
        var delays = new List<TimeSpan>();
        var handler = new QueueHttpMessageHandler(
            Respond(HttpStatusCode.TooManyRequests),
            Respond(HttpStatusCode.TooManyRequests),
            Respond(HttpStatusCode.TooManyRequests));
        var client = CreateClient(handler, apiKey, delays);

        var exception = await ExpectThrows<GoogleBooksApiException>(
            () => client.CallApiAsync("retry", "1", CancellationToken.None));

        AssertTrue(exception.IsRateLimited, "Final 429 must be marked rate limited.");
        AssertEqual(HttpStatusCode.TooManyRequests, exception.StatusCode, "Status code mismatch.");
        AssertEqual(3, handler.RequestUris.Count, "Retry count must be bounded to three total attempts.");
        AssertSequence(delays, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
        AssertFalse(exception.ToString().Contains(apiKey, StringComparison.Ordinal), "Exception must not contain API key.");
    }

    private static async Task NonRateLimitStatusIsSafeFailure()
    {
        var handler = new QueueHttpMessageHandler(Respond(HttpStatusCode.Forbidden, "provider body must not leak"));
        var client = CreateClient(handler);

        var exception = await ExpectThrows<GoogleBooksApiException>(
            () => client.CallApiAsync("failure", "1", CancellationToken.None));

        AssertFalse(exception.IsRateLimited, "403 must not be marked rate limited.");
        AssertEqual(HttpStatusCode.Forbidden, exception.StatusCode, "Status code mismatch.");
        AssertFalse(exception.ToString().Contains("provider body", StringComparison.Ordinal), "Provider body must not leak.");
    }

    private static async Task TransportFailureIsSafe()
    {
        var handler = new QueueHttpMessageHandler(Throw(new HttpRequestException("transport details must not leak")));
        var client = CreateClient(handler);

        var exception = await ExpectThrows<GoogleBooksApiException>(
            () => client.CallApiAsync("failure", "1", CancellationToken.None));

        AssertFalse(exception.IsRateLimited, "Transport failure must not be rate limited.");
        AssertEqual<HttpStatusCode?>(null, exception.StatusCode, "Transport failure must not invent an upstream status.");
        AssertFalse(exception.ToString().Contains("transport details", StringComparison.Ordinal), "Transport details must not leak.");
    }

    private static async Task MalformedJsonIsSafeFailure()
    {
        var handler = new QueueHttpMessageHandler(Respond(HttpStatusCode.OK, "not-json"));
        var client = CreateClient(handler);

        var exception = await ExpectThrows<GoogleBooksApiException>(
            () => client.CallApiAsync("failure", "1", CancellationToken.None));

        AssertFalse(exception.IsRateLimited, "Malformed JSON must not be rate limited.");
        AssertFalse(exception.ToString().Contains("not-json", StringComparison.Ordinal), "Malformed provider content must not leak.");
    }

    private static async Task CallerCancellationIsPreserved()
    {
        var handler = new QueueHttpMessageHandler(Respond(HttpStatusCode.OK, EmptySearchJson));
        var client = CreateClient(handler);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await ExpectThrows<OperationCanceledException>(
            () => client.CallApiAsync("cancel", "1", cancellation.Token));
    }

    private static ApiClient CreateClient(
        QueueHttpMessageHandler handler,
        string? apiKey = null,
        List<TimeSpan>? delays = null,
        DateTimeOffset? utcNow = null)
    {
        delays ??= new List<TimeSpan>();
        return new ApiClient(
            apiKey,
            new HttpClient(handler),
            (delay, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                delays.Add(delay);
                return Task.CompletedTask;
            },
            () => utcNow ?? DateTimeOffset.UtcNow);
    }

    private static Uri SingleRequest(QueueHttpMessageHandler handler)
    {
        AssertEqual(1, handler.RequestUris.Count, "Expected one request.");
        return handler.RequestUris[0];
    }

    private static Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Respond(
        HttpStatusCode statusCode,
        string content = "",
        RetryConditionHeaderValue? retryAfter = null)
    {
        return (_, _) =>
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
            response.Headers.RetryAfter = retryAfter;
            return Task.FromResult(response);
        };
    }

    private static Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> RespondWithInvalidRetryAfter()
    {
        return (_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            response.Headers.TryAddWithoutValidation("Retry-After", "not-valid");
            return Task.FromResult(response);
        };
    }

    private static Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Throw(Exception exception)
    {
        return (_, _) => Task.FromException<HttpResponseMessage>(exception);
    }

    private static async Task<TException> ExpectThrows<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertFalse(bool condition, string message) => AssertTrue(!condition, message);

    private static void AssertContains(string value, string expected)
    {
        AssertTrue(value.Contains(expected, StringComparison.Ordinal), $"Expected '{value}' to contain '{expected}'.");
    }

    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{message} Expected '{expected}', got '{actual}'.");
        }
    }

    private static void AssertSequence(IReadOnlyList<TimeSpan> actual, params TimeSpan[] expected)
    {
        AssertEqual(expected.Length, actual.Count, "Delay count mismatch.");
        for (var index = 0; index < expected.Length; index++)
        {
            AssertEqual(expected[index], actual[index], $"Delay mismatch at index {index}.");
        }
    }
}

internal sealed class QueueHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>> _responses;

    public QueueHttpMessageHandler(params Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>[] responses)
    {
        _responses = new Queue<Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>>(responses);
    }

    public List<Uri> RequestUris { get; } = new();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.RequestUri == null)
        {
            throw new InvalidOperationException("Request URI was missing.");
        }

        RequestUris.Add(request.RequestUri);

        if (_responses.Count == 0)
        {
            throw new InvalidOperationException("No fake response remains.");
        }

        return _responses.Dequeue()(request, cancellationToken);
    }
}
