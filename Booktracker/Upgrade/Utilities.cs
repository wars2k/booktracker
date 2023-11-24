using System.IO;
using Microsoft.Data.Sqlite;

namespace bookTrackerApi.Upgrades {

    public static class Utilities {

        public static string? CreateBackup(UpgradeTypes.ScriptsToRun row) {

            if (row.Title == null) {
                return null;
            }

            Directory.CreateDirectory("external/db/backups");
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd h:mm:ss").Replace(" ", "_");
            string title = row.Title.Replace(" ", "_");
            string path = $"external/db/backups/{dateTime + "_" + title}.db";
            Console.WriteLine(path);
            String cleanedPath = path.Replace(":","-");
            File.Copy("external/db/database.db", cleanedPath);

            return cleanedPath;


        }

        public static List<UpgradeTypes.ScriptsToRun> FindScriptsToRun() {

            using (SqliteConnection connection = DB.initiateConnection()) {

                string sql = "SELECT id, version, title FROM upgrade_scripts WHERE hasRan = @hasRan";
                using (SqliteCommand command = new SqliteCommand(sql, connection)) {

                    command.Parameters.AddWithValue("@hasRan", 0);
                    using (SqliteDataReader reader = command.ExecuteReader()) {
                        List<UpgradeTypes.ScriptsToRun> rows = new();
                        while (reader.Read()) {
                            UpgradeTypes.ScriptsToRun row = new();
                            row.Id = reader.GetInt32(0);
                            row.Version = reader.GetString(1);
                            row.Title = reader.GetString(2);
                            rows.Add(row);
                        }
                        DB.closeConnection(connection);
                        return rows;
                    }
                }
            }

        }

        public static void MarkComplete(UpgradeTypes.ScriptsToRun script, string? backupPath, string logPath) {

            SqliteConnection connection = DB.initiateConnection();
            string sql = "UPDATE upgrade_scripts SET hasRan=@hasRan, completedDateTime=@completedDateTime, logPath=@logPath, backupPath=@backupPath WHERE id=@id";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", script.Id);
            command.Parameters.AddWithValue("@hasRan", 1);
            command.Parameters.AddWithValue("@completedDateTime", DateTime.Now);
            command.Parameters.AddWithValue("@logPath", logPath);
            command.Parameters.AddWithValue("@backupPath", backupPath);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);

        }

        public static string GetLogText(string path) {

            if (File.Exists(path)) {
                string fileContent = File.ReadAllText(path);
                return fileContent;
            } else {
                return "This file does not exist.";
            }

        }

    }

}