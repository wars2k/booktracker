namespace bookTrackerApi {

    public static class EventTypes {

        public enum EventCategories {
            added,
            ratingUpdate,
            statusUpdate,
            progress,
            journal,
            dateStartedUpdate,
            dateFinishedUpdate,
            flag
        }

        public class Internal {
            public int? userID { get; set; }
            public int? bookListID { get; set; }
            public EventCategories? eventType { get; set; }
            public string? value { get; set; }

            public Internal(int UserID, int BookListID, EventCategories EventType, string? Value) {
                userID = UserID;
                bookListID = BookListID;
                eventType = EventType;
                value = Value;
            }
            
        }

        public class External {
            public int? id { get; set; }
            public int? userID { get; set; }
            public int? bookListID { get; set; }
            public string? dateTime { get; set; }
            public string? eventType { get; set; }
            public string? value { get; set; }
        }

    }
}