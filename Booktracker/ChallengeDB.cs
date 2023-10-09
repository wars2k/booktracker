using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace bookTrackerApi {
    
    public static class ChallengeDB {

        public static List<ChallengeTypes.Challenge> getAll(SessionInfo session) {

            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT * FROM challenges WHERE iduser = @iduser";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@iduser", session.AssociatedID);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    List<ChallengeTypes.Challenge> challenges = new List<ChallengeTypes.Challenge>();
                    while (reader.Read()) {
                        ChallengeTypes.Challenge challenge = new ChallengeTypes.Challenge();
                        challenge.Id = reader.GetInt32(0);
                        challenge.Title = reader.GetString(2);
                        challenge.Description = reader.IsDBNull(3) ? null: reader.GetString(3);
                        challenge.Date_created = reader.GetString(4);
                        challenge.Status = reader.IsDBNull(5) ? null: reader.GetString(5);
                        challenge.Type = reader.GetString(6);
                        challenge.SubType = reader.GetString(7);
                        challenge.Start_date = reader.GetString(8);
                        challenge.End_date = reader.GetString(9);
                        challenge.Goal = reader.GetInt32(10);
                        challenge.Count = reader.GetInt32(11);
                        challenge.Record = reader.IsDBNull(12) ? null: reader.GetString(12);
                        challenges.Add(challenge);
                    }
                    DB.closeConnection(connection);
                    return challenges;
                }
            }

        }

        public static void create(ChallengeTypes.NewChallenge challenge, SessionInfo session) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "INSERT INTO challenges (iduser, title, description, date_created, type, subtype, start_date, end_date, goal) VALUES (@iduser, @title, @description, @date_created, @type, @subtype, @start_date, @end_date, @goal)";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@iduser", session.AssociatedID);
            command.Parameters.AddWithValue("@title", challenge.Title);
            command.Parameters.AddWithValue("@description", challenge.Description != null ? challenge.Description : DBNull.Value);
            command.Parameters.AddWithValue("@date_created", DateTime.Now);
            command.Parameters.AddWithValue("@type", challenge.Type);
            command.Parameters.AddWithValue("@subtype", challenge.SubType != null ? challenge.SubType : "");
            command.Parameters.AddWithValue("@start_date", challenge.Start_date);
            command.Parameters.AddWithValue("@end_date", challenge.End_date);
            command.Parameters.AddWithValue("@goal", challenge.Goal);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);
        }

        public static void delete(string challengeID) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "DELETE FROM challenges WHERE id = @id";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", challengeID);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);
        }

        public static void update(ChallengeTypes.LocalChallenge challenge, int newEntry) {
            string record;
            int? count;
            if (challenge.Record == null) {
                record = $"[{newEntry}]";
            } else {
                int closingBracket = challenge.Record.LastIndexOf("]");
                record = challenge.Record.Insert(closingBracket,$",{newEntry}");
            }
            count = challenge.Count + 1;
            SqliteConnection connection = DB.initiateConnection();
            string sql = "UPDATE challenges SET count=@count, record=@record WHERE id=@id";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@count", count);
            command.Parameters.AddWithValue("@record", record);
            command.Parameters.AddWithValue("@id", challenge.Id);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);
        }

        //loops through each challenge in the database, adding each active one to local memory.
        //Should be called everytime there's a change in the challenges table to keep the local memory copy up to date.
        public static void storeChallenges() {

            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT * FROM challenges";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    List<ChallengeTypes.LocalChallenge> challenges = new List<ChallengeTypes.LocalChallenge>();
                    while (reader.Read()) {
                        ChallengeTypes.LocalChallenge challenge = new ChallengeTypes.LocalChallenge();
                        challenge.Id = reader.GetInt32(0);
                        challenge.IdUser = reader.GetInt32(1);
                        challenge.Title = reader.GetString(2);
                        challenge.Description = reader.IsDBNull(3) ? null: reader.GetString(3);
                        challenge.Date_created = reader.GetString(4);
                        challenge.Status = reader.IsDBNull(5) ? null: reader.GetString(5);
                        challenge.Type = reader.GetString(6);
                        challenge.SubType = reader.GetString(7);
                        challenge.Start_date = DateTime.Parse(reader.GetString(8));
                        challenge.End_date = DateTime.Parse(reader.GetString(9));
                        challenge.Goal = reader.GetInt32(10);
                        challenge.Count = reader.GetInt32(11);
                        challenge.Record = reader.IsDBNull(12) ? null: reader.GetString(12);

                        //if the challenge is active, add it to the list which will be stored in local memory.
                        DateTime today = DateTime.Today;
                        if (today <= challenge.End_date) {
                            challenges.Add(challenge);
                        }
                        
                    }
                    DB.closeConnection(connection);
                    Program.ActiveChallenges = challenges;
                }
            }

        }

        public static void handleChallenges(string userID, string type, string subType, int newEntry) {
            List<ChallengeTypes.LocalChallenge> matchingChallenges = findMatchingChallenges(Int32.Parse(userID), type, subType);
            foreach (ChallengeTypes.LocalChallenge challenge in matchingChallenges) {
                bool isValidProgress = IsValidProgress(challenge, newEntry);
                if (isValidProgress) {
                    update(challenge, newEntry);
                    storeChallenges();
                }
            }
        }

        //for a given challenge type and subtype, returns a list of matching challenges.
        public static List<ChallengeTypes.LocalChallenge> findMatchingChallenges(int userID, string type, string subType) {
            List<ChallengeTypes.LocalChallenge> matchingChallenges = new List<ChallengeTypes.LocalChallenge>();
            DateTime today = DateTime.Today;
            foreach (ChallengeTypes.LocalChallenge challenge in Program.ActiveChallenges) {
                
                //if the challenge doesn't belong to this user, skip to the next one;
                if (userID != challenge.IdUser) {
                    continue;
                }

                //if the challenge isn't actually active, skip to the next one;
                if (today > challenge.End_date) {
                    continue;
                }
                
                //if it's a reading challenge, check if type & subtype match before adding to list of matches
                if (challenge.Type == "reading") {
                    if (type == challenge.Type && subType == challenge.SubType) {
                        matchingChallenges.Add(challenge);
                    }

                //if it's a writing challenge, only make sure the type matches before adding to list.
                } else {
                    if (type == challenge.Type) {
                        matchingChallenges.Add(challenge);
                    }
                }
            }

            return matchingChallenges;
        }

        //returns true if a given bookList ID or journal ID should count as progress. Returns false otherwise. 
        public static bool IsValidProgress(ChallengeTypes.LocalChallenge challenge, int newEntry) {

            if (challenge.Record == null) {
                return true;
            }
            //if this item has already been used to make progress, return false
            if (challenge.Record.Contains(newEntry.ToString() + ",") || challenge.Record.Contains(newEntry.ToString() + "]")) {
                return false;
            }

            //if the challenge isn't active anymore
            DateTime today = DateTime.Today;
            if (today < challenge.Start_date || today > challenge.End_date) {
                return false;
            }

            return true;

        }




    }

}