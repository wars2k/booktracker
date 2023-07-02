using CsvHelper;
using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace bookTrackerApi {

    public static class Import {

        
        public static void ImportFromGoodreads(IFormFile file, SessionInfo currentSession) {
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CurrentCulture)) {
                var records = csv.GetRecords<GoodreadsImportRow>(); // Replace YourModelClass with your actual model class
                foreach (var record in records) {
                    DB.addGoodreadsImportedBook(record, currentSession);
                }
            }
        }
    }

    public class GoodreadsImportRow {

        [Name("Book Id")]
        public int? BookID { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        [Name("Author l-f")]
        public string? AuthorReverse { get; set; }
        [Name("Additional Authors")]
        public string? AdditionalAuthors { get; set; }
        public string? ISBN { get; set; }
        public string? ISBN13 { get; set; }
        [Name("My Rating")]
        public int? MyRating { get; set; }
        [Name("Average Rating")]
        public float? AverageRating { get; set; }
        public string? Publisher { get; set; }
        public string? Binding { get; set; }
        [Name("Number of Pages")]
        public int? PageCount { get; set; }
        [Name("Year Published")]
        public int? PublishedDate { get; set; }
        [Name("Original Publication Year")]
        public int? OriginalDate { get; set; }
        [Name("Date Read")]
        public string? DateFinished { get; set; }
        [Name("Date Added")]
        public string? DateAdded { get; set; }
        public string? Bookshelves { get; set; }
        [Name("Bookshelves with positions")]
        public string? BookShelvesWithPositions { get; set; }
        [Name("Exclusive Shelf")]
        public string? ExclusiveShelf { get; set; }
        [Name("My Review")]
        public string? MyReview { get; set; }
        public string? Spoiler { get; set; }
        [Name("Private Notes")]
        public string? PrivateNotes { get; set; }
        [Name("Read Count")]
        public string? ReadCount { get; set; }
        [Name("Owned Copies")]
        public string? OwnedCopies { get; set; }
    }
}