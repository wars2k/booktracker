namespace bookTrackerApi {

    using System.Net;
    using System.Net.Http.Headers;
    using Newtonsoft.Json;

    public class ApiClient {

        private const int MaxAttempts = 3;
        private const string VolumesUrl = "https://www.googleapis.com/books/v1/volumes";

        private static ApiClient _default = new ApiClient();

        private readonly HttpClient _client;
        private readonly string? _apiKey;
        private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync;
        private readonly Func<DateTimeOffset> _utcNow;

        public ApiClient(
            string? apiKey = null,
            HttpClient? client = null,
            Func<TimeSpan, CancellationToken, Task>? delayAsync = null,
            Func<DateTimeOffset>? utcNow = null)
        {
            _apiKey = string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim();
            _client = client ?? new HttpClient();
            _delayAsync = delayAsync ?? Task.Delay;
            _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        }

        public static int Results = 12;

        public static void ConfigureDefault(ApiClient client)
        {
            _default = client ?? throw new ArgumentNullException(nameof(client));
        }

        public static Task<List<object>> CallApiAsync(string name, string results)
        {
            return _default.CallApiAsync(name, results, CancellationToken.None);
        }

        public async Task<List<object>> CallApiAsync(
            string name,
            string results,
            CancellationToken cancellationToken = default)
        {
            var json = await GetJsonAsync(BuildSearchUri(name, results), cancellationToken);

            try {
                var result = JsonConvert.DeserializeObject<GoogleBooksResponse>(json)
                    ?? throw new GoogleBooksApiException();
                var books = new List<object>();

                if (result.Items == null) {
                    return books;
                }

                foreach (var item in result.Items) {
                    if (item?.VolumeInfo == null) {
                        throw new GoogleBooksApiException();
                    }

                    var book = new VolumeInfoSimple {
                        Title = item.VolumeInfo.Title,
                        Author = item.VolumeInfo.Authors != null ? string.Join(", ", item.VolumeInfo.Authors) : null,
                        Publisher = item.VolumeInfo.Publisher,
                        PublishedDate = item.VolumeInfo.PublishedDate,
                        ImageLink = item.VolumeInfo.ImageLinks?.Thumbnail,
                        Id = item.Id,
                        Description = item.VolumeInfo.Description,
                        Isbn = item.VolumeInfo.IndustryIdentifiers,
                        PageCount = item.VolumeInfo.PageCount,
                        Categories = item.VolumeInfo.Categories
                    };
                    books.Add(book);
                }

                return books;
            } catch (JsonException) {
                throw new GoogleBooksApiException();
            }
        }

        public async Task<VolumeInfo> GetBookFromID(
            string id,
            CancellationToken cancellationToken = default)
        {
            var json = await GetJsonAsync(BuildDetailUri(id), cancellationToken);

            try {
                var result = JsonConvert.DeserializeObject<Item>(json);
                if (result?.VolumeInfo == null) {
                    throw new GoogleBooksApiException();
                }

                return new VolumeInfo {
                    Title = result.VolumeInfo.Title,
                    Authors = result.VolumeInfo.Authors,
                    Publisher = result.VolumeInfo.Publisher,
                    PublishedDate = result.VolumeInfo.PublishedDate,
                    ImageLinks = new ImageLinks {
                        Thumbnail = result.VolumeInfo.ImageLinks?.Thumbnail,
                    },
                    Description = result.VolumeInfo.Description,
                    PageCount = result.VolumeInfo.PageCount,
                    IndustryIdentifiers = result.VolumeInfo.IndustryIdentifiers,
                    Categories = result.VolumeInfo.Categories
                };
            } catch (JsonException) {
                throw new GoogleBooksApiException();
            }
        }

        private async Task<string> GetJsonAsync(Uri requestUri, CancellationToken cancellationToken)
        {
            for (var attempt = 0; attempt < MaxAttempts; attempt++) {
                try {
                    using var response = await _client.GetAsync(
                        requestUri,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);

                    if (response.StatusCode == HttpStatusCode.TooManyRequests) {
                        var retryDelay = GetRetryDelay(response.Headers.RetryAfter, attempt);

                        if (attempt == MaxAttempts - 1) {
                            throw new GoogleBooksApiException(response.StatusCode, retryDelay);
                        }

                        await _delayAsync(retryDelay, cancellationToken);
                        continue;
                    }

                    if (!response.IsSuccessStatusCode) {
                        throw new GoogleBooksApiException(response.StatusCode);
                    }

                    return await response.Content.ReadAsStringAsync(cancellationToken);
                } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                    throw;
                } catch (GoogleBooksApiException) {
                    throw;
                } catch (HttpRequestException) {
                    throw new GoogleBooksApiException();
                } catch (OperationCanceledException) {
                    throw new GoogleBooksApiException();
                } catch (IOException) {
                    throw new GoogleBooksApiException();
                }
            }

            throw new GoogleBooksApiException(HttpStatusCode.TooManyRequests);
        }

        private TimeSpan GetRetryDelay(RetryConditionHeaderValue? retryAfter, int attempt)
        {
            if (retryAfter?.Delta is TimeSpan delta && delta > TimeSpan.Zero) {
                return delta;
            }

            if (retryAfter?.Date is DateTimeOffset retryDate) {
                var dateDelay = retryDate - _utcNow();
                if (dateDelay > TimeSpan.Zero) {
                    return dateDelay;
                }
            }

            return TimeSpan.FromSeconds(Math.Pow(2, attempt));
        }

        private Uri BuildSearchUri(string name, string results)
        {
            var query = new List<string> {
                $"q={Uri.EscapeDataString(name ?? string.Empty)}",
                $"maxResults={Uri.EscapeDataString(results ?? string.Empty)}"
            };
            AddApiKey(query);
            return new Uri($"{VolumesUrl}?{string.Join("&", query)}");
        }

        private Uri BuildDetailUri(string id)
        {
            var query = new List<string>();
            AddApiKey(query);
            var suffix = query.Count == 0 ? string.Empty : $"?{string.Join("&", query)}";
            return new Uri($"{VolumesUrl}/{Uri.EscapeDataString(id ?? string.Empty)}{suffix}");
        }

        private void AddApiKey(List<string> query)
        {
            if (_apiKey != null) {
                query.Add($"key={Uri.EscapeDataString(_apiKey)}");
            }
        }
    }

    public sealed class GoogleBooksApiException : Exception {

        public GoogleBooksApiException(
            HttpStatusCode? statusCode = null,
            TimeSpan? retryAfter = null)
            : base(statusCode == HttpStatusCode.TooManyRequests
                ? "Google Books request was rate limited."
                : "Google Books request failed.")
        {
            StatusCode = statusCode;
            RetryAfter = retryAfter;
        }

        public HttpStatusCode? StatusCode { get; }
        public TimeSpan? RetryAfter { get; }
        public bool IsRateLimited => StatusCode == HttpStatusCode.TooManyRequests;
    }

    public class GoogleBooksResponse {
        public List<Item>? Items { get; set; }
    }

    public class GoogleBooksResponseSecond {
        public Item? Test { get; set; }
    }

    public class Item {
        public VolumeInfo? VolumeInfo { get; set; }
        public string? Id { get; set; }
    }

    public class VolumeInfo {
        public string? Title { get; set; }
        public List<string>? Authors { get; set; }
        public string? Publisher { get; set; }
        public string? PublishedDate { get; set; }
        public ImageLinks? ImageLinks { get; set; }
        public string? Description { get; set; }
        public string? PageCount { get; set; }
        public List<IndustryIdentifiers>? IndustryIdentifiers { get; set; }
        public List<string>? Categories { get; set; }
    }

    public class VolumeInfoSimple {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public string? PublishedDate { get; set; }
        public string? ImageLink { get; set; }
        public string? Description { get; set; }
        public string? PageCount { get; set; }
        public string? Id { get; set; }
        public List<IndustryIdentifiers>? Isbn { get; set; }
        public List<string>? Categories { get; set; }
    }

    public class ImageLinks {
        public string? Thumbnail { get; set; }
    }

    public class IndustryIdentifiers {
        public string? Type { get; set; }
        public string? Identifier { get; set; }
    }

    public class Categories {
        public string? Category { get; set; }
    }
}
