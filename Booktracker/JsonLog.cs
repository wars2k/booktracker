namespace bookTrackerApi {

    public static class JsonLog {
        private static string logFilePath = "external/log/log.txt";

        public static void writeLog(string message, string urgency, string eventType, SessionInfo? sessionInfo, string? remoteIP) {
            if (Program.loggingLevel == LoggingLevels.error_only) {
                if (urgency == "INFO" || urgency == "WARNING") {
                    return;
                }
            } else if (Program.loggingLevel == LoggingLevels.error_and_warning) {
                if (urgency == "INFO") {
                    return;
                }
            }
            if (remoteIP == null) {
                remoteIP = "null";
            }
            string username;
            string userID;
            if (sessionInfo == null) {
                username = "null";
                userID = "null";
            } else {
                username = sessionInfo.Username;
                userID = sessionInfo.AssociatedID;
            }
            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string logEntry = $"{{ \"timestamp\": \"{timestamp}\", \"level\": \"{urgency}\", \"message\": \"{message}\", \"event\": \"{eventType}\", \"user_info\": {{ \"username\": \"{username}\", \"user_id\": \"{userID}\", \"remote_ip\": \"{remoteIP}\"}}}}";
            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }

        public static ErrorMessage logAndCreateErrorMessage(ErrorMessage errorMessage, string eventType, SessionInfo? currentSession, string? remoteIP) {
            writeLog(errorMessage.Message, "ERROR", eventType, currentSession, remoteIP);
            return errorMessage;
        }

    }

}