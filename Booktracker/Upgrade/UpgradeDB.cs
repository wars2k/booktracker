using Microsoft.Data.Sqlite;

namespace bookTrackerApi.Upgrades {

    public static class UpgradeDB {

        public static List<UpgradeTypes.ScriptInfo> GetAllUpgrades() {

            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT * FROM upgrade_scripts WHERE hasRan = 1";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    List<UpgradeTypes.ScriptInfo> upgrades = new();
                    while (reader.Read()) {
                        UpgradeTypes.ScriptInfo upgrade = new();
                        upgrade.Id = reader.GetInt32(0);
                        upgrade.Version = reader.GetString(1);
                        upgrade.Title = reader.GetString(2);
                        upgrade.Description = reader.GetString(3);
                        upgrade.CompletedDateTime = reader.GetString(5);
                        upgrade.LogPath = reader.GetString(6);
                        upgrade.BackupPath = reader.GetString(7);
                        upgrades.Add(upgrade);
                    }
                    DB.closeConnection(connection);
                    return upgrades;
                }
            }

        }

        public static UpgradeTypes.ScriptInfo GetUpgradeByID(int id) {

            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT * FROM upgrade_scripts WHERE id = @id";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@id", id);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    UpgradeTypes.ScriptInfo upgrade = new();
                    while (reader.Read()) {
                        
                        upgrade.Id = reader.GetInt32(0);
                        upgrade.Version = reader.GetString(1);
                        upgrade.Title = reader.GetString(2);
                        upgrade.Description = reader.GetString(3);
                        upgrade.CompletedDateTime = reader.GetString(5);
                        upgrade.LogPath = reader.GetString(6);
                        upgrade.BackupPath = reader.GetString(7);
                    }
                    DB.closeConnection(connection);
                    return upgrade;
                }
            }

        }

    }

}