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
            command.Parameters.AddWithValue("@value", eventDef.value);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);
        }

        public static List<EventTypes.External> Get(int bookListID) {
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
                        singleEvent.value = reader.GetString(5);
                        events.Add(singleEvent);

                    }
                    DB.closeConnection(connection);
                    return events;
                }
            }
        }

    }

}