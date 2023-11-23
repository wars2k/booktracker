namespace bookTrackerApi.Upgrades {

    public static class EntryPoint {

        public static void HandleUpgrades() {

            //first, we want to add any new upgrade scripts to the database if there are any.
            ScriptDatabaseUpdater.AddMissingRows();

            //get the scripts that have not ran

            
            //for each one, do the following:
            //  1. Create a backup of the database and return the path
            //  2. Call the method that maps a script ID to a script entry point. This should return a path to the log file.
            //  3. Update the row with dateTime, log path, backup path, etc. 
        }

    }

}