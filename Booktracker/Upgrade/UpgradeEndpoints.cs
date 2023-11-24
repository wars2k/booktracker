using MySql.Data.MySqlClient.Replication;
using Newtonsoft.Json;

namespace bookTrackerApi.Upgrades {

    public static class UpgradeEndpoints {

        public static void Configure(WebApplication app) {

            app.MapGet("/api/settings/upgrades", async (HttpContext context, string sessionKey) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "challenges_view", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                if (currentSession.IsAdmin == 0) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_privileges, "upgrade_view", currentSession, remoteIp);
                    return Results.Unauthorized();
                }
                List<UpgradeTypes.ScriptInfo> upgradeScripts = UpgradeDB.GetAllUpgrades();
                return Results.Ok(upgradeScripts);
            });

            app.MapGet("/api/settings/upgrades/{id}", async (HttpContext context, string sessionKey, int id) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "challenges_view", null, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                if (currentSession.IsAdmin == 0) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_privileges, "upgrade_view", currentSession, remoteIp);
                    return Results.Unauthorized();
                }
                UpgradeTypes.UpgradeInfo response = new();
                response.ScriptInfo = UpgradeDB.GetUpgradeByID(id);
                if (response.ScriptInfo.Id == null) {
                    return Results.NotFound();
                }
                long fileSizeInBytes = new FileInfo(response.ScriptInfo.BackupPath).Length;
                double fileSizeInMB = (double)fileSizeInBytes / (1024 * 1024);
                response.BackupSize = fileSizeInMB.ToString() + " mb";
                response.LogText = Utilities.GetLogText(response.ScriptInfo.LogPath);
                return Results.Ok(response);
            });

        }

    }

}