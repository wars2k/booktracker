using Microsoft.Data.Sqlite;

namespace bookTrackerApi {

    public static class SettingsDB {

        public static void updateLoggingLevel(string level) {

            SqliteConnection connection = DB.initiateConnection();
            string sql = "UPDATE settings SET value = @level WHERE name = 'logging_level'";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@level", level);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);
            Program.loggingLevel = level;
        }

        public static string getLoggingLevel() {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT * FROM settings WHERE name = 'logging_level'";
            SqliteCommand command = new SqliteCommand(sql, connection);
            using (SqliteDataReader reader = command.ExecuteReader()) {
                    string? logging_level = null;
                    while (reader.Read()) {
                        logging_level = reader.GetString(1);   
                    }
                    DB.closeConnection(connection);
                    return logging_level;
                }
        }

    }

    public static class LoggingLevels {

        public static string all = "all";
        public static string error_only = "error_only";
        public static string error_and_warning = "error_and_warning";

    }

}    