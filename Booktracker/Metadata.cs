using Newtonsoft.Json;

namespace bookTrackerApi;

public class ApiClient
{
    public static int Results = 12;

    private readonly HttpClient _client;

    public ApiClient()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("https://www.googleapis.com");
    }


    public static async Task<List<object>> CallApiAsync(string name)
    {
        HttpClient client = new HttpClient();
        HttpResponseMessage response =
            await client.GetAsync("https://www.googleapis.com/books/v1/volumes?q=" + name + $"&maxResults={Results}");
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        GoogleBooksResponse? result = JsonConvert.DeserializeObject<GoogleBooksResponse>(json);

        List<object> books = new List<object>();
        foreach (Item item in result.Items)
        {
            VolumeInfoSimple book = new VolumeInfoSimple
            {
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
    }

    public static async Task<VolumeInfo> GetBookFromID(string id)
    {
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync("https://www.googleapis.com/books/v1/volumes/" + id);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        Item? result = JsonConvert.DeserializeObject<Item>(json);


        VolumeInfo book = new VolumeInfo
        {
            Title = result.VolumeInfo.Title,
            Authors = result.VolumeInfo.Authors,
            Publisher = result.VolumeInfo.Publisher,
            PublishedDate = result.VolumeInfo.PublishedDate,
            ImageLinks = new ImageLinks
            {
                Thumbnail = result.VolumeInfo.ImageLinks?.Thumbnail
            },
            Description = result.VolumeInfo.Description,
            PageCount = result.VolumeInfo.PageCount,
            IndustryIdentifiers = result.VolumeInfo.IndustryIdentifiers,
            Categories = result.VolumeInfo.Categories
        };
        string stringBook = JsonConvert.SerializeObject(book);
        Log.writeLog(stringBook, "SPECIAL INFO");
        return book;
    }
}

public class GoogleBooksResponse
{
    public List<Item>? Items { get; set; }
}

public class GoogleBooksResponseSecond
{
    public Item? Test { get; set; }
}

public class Item
{
    public VolumeInfo? VolumeInfo { get; set; }
    public string? Id { get; set; }
}

public class VolumeInfo
{
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

public class VolumeInfoSimple
{
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

public class ImageLinks
{
    public string? Thumbnail { get; set; }
}

public class IndustryIdentifiers
{
    public string? Type { get; set; }
    public string? Identifier { get; set; }
}

public class Categories
{
    public string? Category { get; set; }
}