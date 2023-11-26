namespace bookTrackerApi.Upgrades {

    public static class EntryPoint {
        
        ///<summary>Called on startup. Does the following: Adds missing upgrade rows to the database, finds which ones have not yet run,
        ///loops through each one that should run, creates backups, runs the upgrade, then documents that it has ran in the database.</summary>
        public static void HandleUpgrades() {

            //first, we want to add any new upgrade scripts to the database if there are any.
            ScriptDatabaseUpdater.AddMissingRows();

            //get the scripts that have not ran
            List<UpgradeTypes.ScriptsToRun> scriptsToRun = Utilities.FindScriptsToRun();

            foreach (UpgradeTypes.ScriptsToRun script in scriptsToRun) {

                if (script == null) {
                    continue;
                }
                if (ScriptFinder(script.Title, false) != "found") {
                    continue;
                }
                string? backupPath = Utilities.CreateBackup(script);

                string logPath = ScriptFinder(script.Title, true);
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


        ///<summary>Using the upgrade script's title, determines which code to run when performing an upgrade.</summary>
        ///<param name="title">The title of the upgrade script.</param>
        ///<param name="runScript">Set to true if the script should actually run if it's found. If 'false', it will
        ///return 'found' rather than actually running the script.</param>
        public static string ScriptFinder(string? title, Boolean runScript) {

            string pathToLog;

            switch (title) {
                
                default:
                    return "notFound";
            }

            return pathToLog;
        }

    }

}