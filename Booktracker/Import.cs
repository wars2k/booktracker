using CsvHelper;
using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using Microsoft.Data.Sqlite;

namespace bookTrackerApi {

    public static class Import {

        
        public static async Task ImportFromGoodreads(IFormFile file, SessionInfo currentSession) {
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CurrentCulture)) {
                var records = csv.GetRecords<GoodreadsImportRow>(); // Replace YourModelClass with your actual model class
                foreach (var record in records) {
                    int bookListID = await DB.addGoodreadsImportedBook(record, currentSession);
                    string ID = bookListID.ToString();
                    Console.WriteLine($"[{ID}] Success.");
                    if (record.Bookshelves != "") {
                        List<int> listOfCollectionIDs = HandleImportedBookshelves(record.Bookshelves, currentSession);
                        AddBookToCollection(bookListID, listOfCollectionIDs);
                    }
                }
            }
        }

        public static List<int> HandleImportedBookshelves(string bookshelves, SessionInfo currentSession) {
            List<string> bookshelfList = CreateBookshelfList(bookshelves); //turn the bookshelves into a list
            List<int> listOfCollectionIDs = new List<int>();
            for (int i = 0; i < bookshelfList.Count; i++) {
                int? id = GetCollectionIDFromName(bookshelfList[i]);
                if (id == null) {
                    APITypes.newCollectionRequestBody info = new APITypes.newCollectionRequestBody();
                    info.Name = bookshelfList[i];
                    info.Description = null;
                    info.CoverImage = null;
                    CollectionsDB.createNew(info, currentSession);
                    int? collectionID = GetCollectionIDFromName(bookshelfList[i]);
                    int collectionIDRegular = collectionID ?? -1;
                    listOfCollectionIDs.Add(collectionIDRegular);
                } else if (id != null) {
                    int regularID = id ?? -1;
                    listOfCollectionIDs.Add(regularID);
                }
            }
            return listOfCollectionIDs;
        }

        public static List<string> CreateBookshelfList(string bookshelves) {
            string delimiter = ", ";
            List<string> bookshelfList = bookshelves.Split(delimiter).ToList();
            return bookshelfList;
        }

        public static int? GetCollectionIDFromName(string bookshelf) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT * FROM collections WHERE collection_name = @name";
            int? id = null;
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@name", bookshelf);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        id = reader.IsDBNull(0) ? null: reader.GetInt32(0);
                    }
                    DB.closeConnection(connection);
                
                }
            }
            return id;
        }

        public static void AddBookToCollection(int bookListID, List<int> CollectionIDs) {
            for (int i = 0; i < CollectionIDs.Count; i++) {
                CollectionsDB.addBook(CollectionIDs[i], bookListID);
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