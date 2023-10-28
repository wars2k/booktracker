using Microsoft.Data.Sqlite;

namespace bookTrackerApi {

    public static class ProgressDB {

        //adds a new progress update to the database. 
        //Returns: the ID of the newly inserted row. 
        public static int Create(ProgressTypes.Internal progressData) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "INSERT INTO progress_updates (iduser, idbookList, dateTime, currentPosition, journalID, comment) VALUES (@iduser, @idbookList, @dateTime, @currentPosition, @journalID, @comment)";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@iduser", progressData.UserID);
            command.Parameters.AddWithValue("@idbookList", progressData.BookListID);
            command.Parameters.AddWithValue("@dateTime", progressData.DateTime);
            command.Parameters.AddWithValue("@currentPosition", progressData.CurrentPosition);
            command.Parameters.AddWithValue("@journalID", progressData.Journal != null ? progressData.Journal : DBNull.Value);
            command.Parameters.AddWithValue("@comment", progressData.Comment != null ? progressData.Comment : DBNull.Value);
            command.ExecuteNonQuery();

            command.CommandText = "SELECT last_insert_rowid()";
            int lastInsertedID = Convert.ToInt32(command.ExecuteScalar());

            DB.closeConnection(connection);

            return lastInsertedID;
        }


        //gets a single update from the DB by ID and returns it. 
        //Use Case: API endpoint calls this when a user requests a specific update. 
        public static ProgressTypes.External GetOne(int updateID) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT * FROM progress_updates WHERE id = @id";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@id", updateID);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    ProgressTypes.External update = new();
                    while (reader.Read()) {
                        update.Id = reader.GetInt32(0);
                        update.DateTime = reader.GetString(1);
                        update.CurrentPosition = reader.GetInt32(2);
                        update.Journal = reader.IsDBNull(3) ? null: reader.GetInt32(3);
                        update.Comment = reader.IsDBNull(4) ? null: reader.GetString(4);
                    }
                    DB.closeConnection(connection);
                    return update;
                }
            }
        }


        //gets all progress events for a given bookList ID and returns it.
        public static List<ProgressTypes.External> GetAll(int bookListID) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT * FROM progress_updates WHERE idbookList = @idbookList";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@idbookList", bookListID);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    List<ProgressTypes.External> updateList = new();
                    while (reader.Read()) {
                        ProgressTypes.External update = new();
                        update.Id = reader.GetInt32(0);
                        update.DateTime = reader.GetString(1);
                        update.CurrentPosition = reader.GetInt32(2);
                        update.Journal = reader.IsDBNull(3) ? null: reader.GetInt32(3);
                        update.Comment = reader.IsDBNull(4) ? null: reader.GetString(4);
                        updateList.Add(update);
                    }
                    DB.closeConnection(connection);
                    return updateList;
                }
            }
        }

    }

}