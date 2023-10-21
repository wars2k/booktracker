using Microsoft.Data.Sqlite;

namespace bookTrackerApi {

    public static class EventDB {

        public static void Add(EventTypes.Internal eventDef) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "INSERT INTO book_events (iduser, idbookList, dateTime, event, value) VALUES (@iduser, @idbookList, @dateTime, @event, @value)";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@iduser", eventDef.userID);
            command.Parameters.AddWithValue("@idbookList", eventDef.bookListID);
            command.Parameters.AddWithValue("@dateTime", DateTime.Now);
            command.Parameters.AddWithValue("@event", eventDef.eventType.ToString());
            if (eventDef.value != null) {
                command.Parameters.AddWithValue("@value", eventDef.value);
            } else {
                command.Parameters.AddWithValue("@value", DBNull.Value);
            }
            command.ExecuteNonQuery();
            DB.closeConnection(connection);
        }

        public static List<EventTypes.External> GetEvents(int bookListID) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT * FROM book_events WHERE idbookList = @idbookList";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@idbookList", bookListID);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    List<EventTypes.External> events = new List<EventTypes.External>();
                    while (reader.Read()) {
                        EventTypes.External singleEvent = new EventTypes.External();
                        singleEvent.id = reader.GetInt32(0);
                        singleEvent.userID = reader.GetInt32(1);
                        singleEvent.bookListID = reader.GetInt32(2);
                        singleEvent.dateTime = reader.GetString(3);
                        singleEvent.eventType = reader.GetString(4);
                        singleEvent.value = reader.IsDBNull(5) ? null: reader.GetString(5);
                        events.Add(singleEvent);

                    }
                    DB.closeConnection(connection);
                    return events;
                }
            }
        }
        
        //called anytime there's a bookListUpdate. Makes events where necessary. 
        public static void handleBookListEvents(Api.BookListData data, int userID, int bookListID) {
            if (data.Status != null) {
                EventTypes.Internal statusEvent = new EventTypes.Internal(userID, bookListID, EventTypes.EventCategories.statusUpdate, data.Status);
                Add(statusEvent);
            }

            if (data.Rating != null) {
                EventTypes.Internal ratingEvent = new EventTypes.Internal(userID, bookListID, EventTypes.EventCategories.ratingUpdate, data.Rating);
                Add(ratingEvent);
            }

            if (data.StartDate != null) {
                EventTypes.Internal startDateEvent = new EventTypes.Internal(userID, bookListID, EventTypes.EventCategories.dateStartedUpdate, data.StartDate);
                Add(startDateEvent);
            }

            if (data.FinishedDate != null) {
                EventTypes.Internal finishedDateEvent = new EventTypes.Internal(userID, bookListID, EventTypes.EventCategories.dateFinishedUpdate, data.FinishedDate);
                Add(finishedDateEvent);
            }
        }

    }

}