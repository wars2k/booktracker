namespace bookTrackerApi.Upgrades {

    public static class EntryPoint {

        public static void HandleUpgrades() {

            //first, we want to add any new upgrade scripts to the database if there are any.
            ScriptDatabaseUpdater.AddMissingRows();

            //get the scripts that have not ran
            List<UpgradeTypes.ScriptsToRun> scriptsToRun = Utilities.FindScriptsToRun();

            foreach (UpgradeTypes.ScriptsToRun script in scriptsToRun) {

                if (script == null) {
                    continue;
                }
                string? backupPath = Utilities.CreateBackup(script);

                string logPath = ScriptFinder(script.Title);
                if (logPath == "notFound") {
                    continue;
                }

                Utilities.MarkComplete(script, backupPath, logPath);
            }
            
            //for each one, do the following:
            //  1. Create a backup of the database and return the path
            //  2. Call the method that maps a script ID to a script entry point. This should return a path to the log file.
            //  3. Update the row with dateTime, log path, backup path, etc. 
        }



        public static string ScriptFinder(string? title) {

            string pathToLog;

            switch (title) {
                case "Test upgrade script":
                    pathToLog = "path/to/log";
                    break;
                case "A different upgrade script":
                    pathToLog = "path/to/log2";
                    break;
                default:
                    return "notFound";
            }

            return pathToLog;
        }

    }

}