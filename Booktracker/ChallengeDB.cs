using Microsoft.Data.Sqlite;

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
            command.Parameters.AddWithValue("@subtype", challenge.SubType);
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

        public static void update(string challengeID, ChallengeTypes.Challenge challenge, int newEntry) {
            string record;
            int? count;
            if (challenge.Record == null) {
                record = $"[{newEntry}]";
            } else {
                int closingBracket = challenge.Record.LastIndexOf("]");
                record = challenge.Record.Insert(closingBracket,$",{newEntry}");
            }
            count = challenge.Count + 1;
        }

    }

}