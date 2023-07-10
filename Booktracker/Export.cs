using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace bookTrackerApi {

    public static class Export {

        public static void ExportDataAsJSON(List<DB.BookPageInfo> listOfBooks, string username) {

            string jsonString = JsonSerializer.Serialize(listOfBooks, new JsonSerializerOptions {
                WriteIndented = true
            });

            File.WriteAllText($"../external/{username}-export.json", jsonString);

        }

        // public static void ExportDataAsCSV(List<DB.BookPageInfo> listOfBooks, string username) {

        //     using (StreamWriter writer = new StreamWriter($"../external/{username}-export.csv")) {

        //         string columns = "id,bookID,title,author,publishedDate,publisher,imageLink,status,rating,dateStarted,dateFinished,description,pageCount,isbn,category";
        //         writer.WriteLine(columns);
        //         for (int i = 0; i < listOfBooks.Count; i++) {
        //             DB.BookPageInfo book = listOfBooks[i];
        //             string row = "";
        //             row += book.Id + "," + book.BookID + ",";
        //             row += $"\"{book.Title}\",\"{book.Author}\",\"{book.PublishedDate}\",\"{book.Publisher}\",\"{book.ImageLink}\",";
        //             row += $"\"{book.Status}\",\"{book.Rating}\",\"{book.DateStarted}\",\"{book.DateFinished}\",\"{book.Description}\",\"{book.PageCount}\",\"{book.Isbn}\",\"{book.Category}\"";
        //             writer.WriteLine(row);

        //         }
        //     }
        // }

        public static void ExportDataAsCSV(List<DB.BookPageInfo> listOfBooks, string username)
        {
            using (StreamWriter writer = new StreamWriter($"../external/{username}-export.csv"))
            {
                string columns = "id,bookID,title,author,publishedDate,publisher,imageLink,status,rating,dateStarted,dateFinished,description,pageCount,isbn,category";
                writer.WriteLine(columns);

                for (int i = 0; i < listOfBooks.Count; i++)
                {
                    DB.BookPageInfo book = listOfBooks[i];
                    string row = $"{book.Id},{book.BookID},";
                    row += $"\"{EscapeField(book.Title)}\",\"{EscapeField(book.Author)}\",\"{EscapeField(book.PublishedDate)}\",\"{EscapeField(book.Publisher)}\",\"{EscapeField(book.ImageLink)}\",";
                    row += $"\"{EscapeField(book.Status)}\",\"{EscapeField(book.Rating)}\",\"{EscapeField(book.DateStarted)}\",\"{EscapeField(book.DateFinished)}\",\"{EscapeField(book.Description)}\",\"{EscapeField(book.PageCount)}\",\"{EscapeField(book.Isbn)}\",\"{EscapeField(book.Category)}\"";
                    writer.WriteLine(row);
                }
            }
        }

        private static string EscapeField(string field)
        {
        
            return field?.Replace("\"", "\"\"");
        }
    }

}