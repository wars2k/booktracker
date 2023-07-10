namespace bookTrackerApi {

    public static class Log {
        private static string logFilePath = "external/log/log.txt";

        public static void writeLog(string message, string urgency) {
            string logEntry = $"[{urgency}] {DateTime.Now}: {message}";
            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }

        public static void logFailedLoginAttempt(string username) {
            string message = $"Unsuccessful login attempt. Username inputted: {username}";
            writeLog(message, "WARNING");
        }

        public static void logSuccessfulLoginAttempt(string username) {
            string message = $"Successful login attempt by user: {username}";
            writeLog(message, "INFO");
        }

        public static void logSuccessfulLogout(SessionInfo sessionInfo) {
            string message = $"Successful logout attempt by user: {sessionInfo.Username} (UID {sessionInfo.AssociatedID}).";
            writeLog(message, "INFO");
        }

        public static void logDeletedBook(string id, SessionInfo sessionInfo) {
            string message = $"Book ID {id} deleted by admin user {sessionInfo.Username}.";
            writeLog(message, "INFO");
        }

        public static void logManualEntry(string title, SessionInfo sessionInfo) {
            string message = $"MANUAL ENTRY: Title: {title} - Added by {sessionInfo.Username} (UID: {sessionInfo.AssociatedID}).";
            writeLog(message, "INFO");
        }

        public static void failedManualEntry(string type, SessionInfo? sessionInfo) {
            if (type == "incorrectSessionKey") {
                string message = "Manual Entry attempt failed due to incorrect sessionKey.";
                writeLog(message, "ERROR");
            } else if (type == "noRequestBody" && sessionInfo != null) {
                string message = $"Manual Entry by '{sessionInfo.Username}' failed due to no body attached to request.";
                writeLog(message, "ERROR"); 
            } else {
                string message = "Manual entry failed due to unknown reasons.";
                writeLog(message, "ERROR");
            }
        }

        public static void AlertFailedBookListRetrieval(string typeOfError, SessionInfo? sessionInfo) {
            string message;
            string? id = "unknown ID";
            if (sessionInfo != null) {
                id = sessionInfo.AssociatedID;
            }
            if (typeOfError == "emptyPayload") {

                message = $"Booklist retrieval for UID {sessionInfo.AssociatedID} failed due to empty payload.";

            } else if (typeOfError == "noBackEndSession") {

                message = $"Booklist retrieval failed due to no current session in the backend."; 

            } else {

                message = $"BookList retrieval for UID {sessionInfo.AssociatedID} failed due to incorrect session key in payload.";

            }
            writeLog(message, "ERROR");
            return;
        }

        public static void externalAPIquery(string title, SessionInfo sessionInfo) {
            string message = $"Google Books API queried for '{title}' by User ID: {sessionInfo.AssociatedID}.";
            writeLog(message, "INFO");
        }

        public static void failedAPIquery(string? title, string type) {
            string message = "placeholder";
            if (type == "missingParameter") {
                message = $"Google Books API query failed due to missing parameters.";
            } else if (type == "incorrectSessionKey") {
                message = $"Google Books API query for '{title}' failed due to incorrect session key.";
            }
            writeLog(message, "ERROR");
        }

        public static void sqlQuery(string sql) {
            writeLog(sql, "INFO");
        }

        public static void AlertFailedBookListEdit(string typeOfError, SessionInfo? sessionInfo) {
            string message;
            string? id = "unknown ID";
            if (sessionInfo != null) {
                id = sessionInfo.AssociatedID;
            }
            if (typeOfError == "emptyPayload") {

                message = $"Booklist edit by UID {id} failed due to empty payload.";

            } else if (typeOfError == "noBackEndSession") {

                message = $"Booklist edit by UID {id} failed due to no current session in the backend."; 

            } else  if (typeOfError == "incorrectSessionKey") {

                message = $"BookList edit by UID {id} failed due to incorrect session key in payload.";

            } else {

                message = $"BookList edit by UID {id} failed due to unknown reasons.";
            }
            writeLog(message, "ERROR");
            return;
        }

        public static void AlertSuccessfulBookListEdit(Api.BookListData data, SessionInfo sessionInfo) {
            string message = $"BookList edited by User ID: {sessionInfo.AssociatedID}. New Values: ";
            if (data.Status != null) {
                message += $"Status: {data.Status}, ";
            }
            if (data.Rating != null) {
                message += $"Rating: {data.Rating}, ";
            }
            if (data.StartDate != null) {
                message += $"Start Date: {data.StartDate}";
            }
            if (data.FinishedDate != null) {
                message += $"Finished Date: {data.FinishedDate}";
            }
            writeLog(message, "INFO");
            return;
            
        }

        public static void AlertFailedBookListDelete(string typeOfError, SessionInfo sessionInfo) {
            string message;
            if (typeOfError == "emptyPayload") {

                message = $"Booklist delete by UID {sessionInfo.AssociatedID} failed due to empty payload.";

            } else if (typeOfError == "noBackEndSession") {

                message = $"Booklist delete by UID {sessionInfo.AssociatedID} failed due to no current session in the backend."; 

            } else  if (typeOfError == "incorrectSessionKey") {

                message = $"BookList dekete by UID {sessionInfo.AssociatedID} failed due to incorrect session key in payload.";

            } else {

                message = $"BookList delete by UID {sessionInfo.AssociatedID} failed due to unknown reasons.";
            }
            writeLog(message, "ERROR");
            return;
        }

        public static void AlertSuccessfulBookListDelete(string id, SessionInfo sessionInfo) {
            string message = $"BookList entry with ID {id} deleted by User ID: {sessionInfo.AssociatedID}.";
            writeLog(message, "INFO");
            return;
            
        }
    }


}