using Microsoft.Data.Sqlite;

namespace bookTrackerApi.Upgrades { 

    public static class Upgrades {

        public static string PreventDuplicateSaves() {

            Utilities.WriteLog("preventDuplicateSaves", "Beginning database operation to add 'gb_id' column to 'books'.");
            string sql = "ALTER TABLE books ADD gb_id TEXT";
            SqliteConnection connection = DB.initiateConnection();
            SqliteCommand command = new SqliteCommand(sql, connection);
            try {
                command.ExecuteNonQuery();
                DB.closeConnection(connection);
                Utilities.WriteLog("preventDuplicateSaves", "Database operation complete.");
                
            } catch(Exception ex) {
                Utilities.WriteLog("preventDuplicateSaves", "An error occurred while performing this operation: " + ex);
                
            }
            
            return "/external/log/preventDuplicateSaves.txt";
        }

    }

}