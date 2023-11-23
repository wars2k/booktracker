namespace bookTrackerApi.Upgrades {

    public static class UpgradeTypes {

        public class JSONScriptInfo {

            public string? Version { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }

        }

        public class ScriptInfo {

            public int? Id { get; set; }
            public string? Version { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public Boolean HasRan { get; set; }
            public string? CompletedDateTime { get; set; }
            public string? LogPath { get; set; }
            public string? BackupPath { get; set; }
        }

        public class ScriptsToRun {

            public int? Id { get; set; }
            public string? Title { get; set; }
            public string? Version { get; set; }
        }

    }

}