namespace bookTrackerApi.Upgrades {

    public static class UpgradeTypes {

        ///<summary>This type is associated with the JSON file of upgrades that need to be
        ///reconciled against the database on startup.</summary>
        public class JSONScriptInfo {

            public string? Version { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }

        }

        ///<summary>Contains most of the information about a given upgrade. Used when returning
        ///a list of finished upgrades to the user from the API.</summary>
        public class ScriptInfo {

            public int? Id { get; set; }
            public string? Version { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? CompletedDateTime { get; set; }
            public string? LogPath { get; set; }
            public string? BackupPath { get; set; }
        }

        ///<summary>Used when determining which scripts need to run. Title + Version
        ///is used to identify the correct code to run when actually doing the ugprade.</summary>
        public class ScriptsToRun {

            public int? Id { get; set; }
            public string? Title { get; set; }
            public string? Version { get; set; }
        }

        ///<summary>Contains the full information about an upgrade including backupSize and the full
        ///text of the logs.</summary>
        public class UpgradeInfo {

            public ScriptInfo? ScriptInfo { get; set; }
            public string? BackupSize { get; set; }
            public string? LogText { get; set; }

        }

    }

}