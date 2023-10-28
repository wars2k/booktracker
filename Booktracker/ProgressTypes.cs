namespace bookTrackerApi {

    public static class ProgressTypes {

        //This is the info that the client sends to the server when creating a new progress event. 
        //UserID is provided by the sessionKey & bookListID is provided in the request URL.
        public class RequestBody {

            public int? currentPosition { get; set; }
            public int? journal { get; set; }
            public string? comment { get; set; }

        }

        //This type holds all the information needed to add a new row to the progress_updates table.
        public class Internal {
            public DateTime DateTime { get; set; }
            public int? BookListID { get; set; }
            public int? UserID { get; set; }
            public int? CurrentPosition { get; set; }
            public int? Journal { get; set; }
            public string? Comment { get; set; }

            public Internal(RequestBody request, int bookListID, int userID) {
                DateTime = DateTime.Now;
                BookListID = bookListID;
                UserID = userID;
                CurrentPosition = request.currentPosition;
                Journal = request.journal;
                Comment = request.comment;
            }

        }

        //This is what the response is from the server when the client requests info about a progress event.
        public class External {
            public int? Id { get; set; }
            public string? DateTime { get; set; }
            public int? CurrentPosition { get; set; }
            public int? Journal { get; set; }
            public string? Comment { get; set; }
        }

    }


}