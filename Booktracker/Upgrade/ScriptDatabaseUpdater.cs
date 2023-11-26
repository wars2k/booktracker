using Newtonsoft.Json;
using Microsoft.Data.Sqlite;

namespace bookTrackerApi.Upgrades {
    
    ///<summary>This class handles all functionality surrounding keeping the upgrade_scripts table up to date.</summary>
    public static class ScriptDatabaseUpdater {
        
        ///<summary>Gets all upgrade script rows from the JSON file and returns them.</summary>
        ///<returns>A lislt of upgrade objects containing title, version, and description.</returns>
        public static List<UpgradeTypes.JSONScriptInfo>? GetAllRows() {

            List<UpgradeTypes.JSONScriptInfo>? upgradeScripts = new();

            string jsonContent = File.ReadAllText("Upgrade/upgrade_scripts.json");

            upgradeScripts = JsonConvert.DeserializeObject<List<UpgradeTypes.JSONScriptInfo>>(jsonContent);

            return upgradeScripts;

        }

        ///<summary>This is the main entry point for this class.
        ///Loops through each row pulled from the JSON. Adds it to the database if it doesn't already exist.</summary>
        public static void AddMissingRows() {
            
            List<UpgradeTypes.JSONScriptInfo>? rows = GetAllRows();

            if (rows == null) {
                return;
            }

            foreach (UpgradeTypes.JSONScriptInfo row in rows) {

                if (!DoesRowExist(row)) {
                    AddRow(row);
                }

            }

        }

        ///<summary>Checks to see if the row exists by searching with version and title.</summary>
        ///<param name="row">The "row" that we are checking to see already exists in the database.</param>
        public static bool DoesRowExist(UpgradeTypes.JSONScriptInfo row) {

            using (SqliteConnection connection = DB.initiateConnection()) {

                string sql = "SELECT COUNT(*) FROM upgrade_scripts WHERE version = @version AND title = @title";
                using (SqliteCommand command = new SqliteCommand(sql, connection)) {

                    command.Parameters.AddWithValue("@version", row.Version);
                    command.Parameters.AddWithValue("@title", row.Title);
                    int rowCount = Convert.ToInt32(command.ExecuteScalar());

                    return rowCount > 0;
                }
            }
        }


        ///<summary>Adds a new row to the database in the 'upgrade_scripts' table. Includes version, title, and description.</summary>
        ///<param name="row">The row that will be added to the database</param>
        public static void AddRow(UpgradeTypes.JSONScriptInfo row) {

            using (SqliteConnection connection = DB.initiateConnection()) {

                string sql = "INSERT INTO upgrade_scripts (version, title, description) VALUES (@version, @title, @description)";
                using (SqliteCommand command = new(sql, connection)) {

                    command.Parameters.AddWithValue("@version", row.Version);
                    command.Parameters.AddWithValue("@title", row.Title);
                    command.Parameters.AddWithValue("@description", row.Description);
                    command.ExecuteNonQuery();

                }
            }

        }



    }

}