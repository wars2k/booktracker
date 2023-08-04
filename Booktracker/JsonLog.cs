namespace bookTrackerApi {

    public static class JsonLog {
        private static string logFilePath = "external/log/jsonLog.txt";

        public static void writeLog(string message, string urgency, string eventType, SessionInfo? sessionInfo, string? remoteIP) {
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