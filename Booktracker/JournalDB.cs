using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Data.Sqlite;

namespace bookTrackerApi {

    public static class JournalDB {

        public static List<JournalTypes.JournalEntryList> getJournalEntries(string bookID, string userID) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT * FROM journal_entries WHERE iduser = @iduser AND idbooklist = @idbooklist";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@iduser", userID);
                command.Parameters.AddWithValue("@idbooklist", bookID);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    List<JournalTypes.JournalEntryList> entries = new List<JournalTypes.JournalEntryList>();
                    while (reader.Read()) {
                        JournalTypes.JournalEntryList entry = new JournalTypes.JournalEntryList();
                        entry.id = reader.GetInt32(0);
                        entry.idUser = reader.GetInt32(1);
                        entry.idBookList = reader.GetInt32(2);
                        entry.dateCreated = reader.IsDBNull(3) ? null: reader.GetString(3);
                        entry.lastEdited = reader.IsDBNull(4) ? null: reader.GetString(4);
                        entry.title = reader.GetString(5);
                        entry.htmlContent = reader.GetString(6);
                        entries.Add(entry);
                    }
                    DB.closeConnection(connection);
                    return entries;
                }
            }
            
        }

        public static void createEntry(JournalTypes.NewEntry entry, SessionInfo sessionInfo, string bookID) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "INSERT INTO journal_entries (iduser, idbooklist, date_created, last_edited, title, html_content) VALUES (@iduser, @idbooklist, @date_created, @last_edited, @title, @html_content)";
            SqliteCommand command = new SqliteCommand(sql, connection);
            DateTime currentDateTime = DateTime.Now;
            command.Parameters.AddWithValue("@iduser", sessionInfo.AssociatedID);
            command.Parameters.AddWithValue("@idbooklist", bookID);
            command.Parameters.AddWithValue("@date_created", currentDateTime);
            command.Parameters.AddWithValue("@last_edited", currentDateTime);
            command.Parameters.AddWithValue("@title", entry.title);
            command.Parameters.AddWithValue("@html_content", entry.htmlContent);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);
        }

        public static void updateEntry(JournalTypes.NewEntry updatedInfo, string journalID) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "UPDATE journal_entries SET last_edited = @last_edited, title = @title, html_content = @html_content WHERE id = @id";
            SqliteCommand command = new SqliteCommand(sql, connection);
            DateTime currentDateTime = DateTime.Now;
            command.Parameters.AddWithValue("@last_edited", currentDateTime);
            command.Parameters.AddWithValue("@title", updatedInfo.title);
            command.Parameters.AddWithValue("@html_content", updatedInfo.htmlContent);
            command.Parameters.AddWithValue("@id", journalID);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);

        }

        public static void deleteEntry(string journalID) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "DELETE FROM journal_entries WHERE id = @id";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", journalID);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);
        }
    }
}