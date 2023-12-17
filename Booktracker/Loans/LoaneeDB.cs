using CsvHelper;
using Microsoft.Data.Sqlite;

namespace bookTrackerApi.Loans {

    public static class LoaneeDB {

        ///<summary>Adds a new loanee to the database and returns their newly created ID</summary>
        public static int AddLoanee(Types.NewLoanee loaneeInfo, int userID) {

            using (SqliteConnection connection = DB.initiateConnection()) {

                string sql = "INSERT INTO loanees (iduser, name, email, phone, note) VALUES (@iduser, @name, @email, @phone, @notes)";
                using (SqliteCommand command = new(sql, connection)) {

                    command.Parameters.AddWithValue("@iduser", userID);
                    command.Parameters.AddWithValue("@name", loaneeInfo.Name);
                    command.Parameters.AddWithValue("@email", loaneeInfo.Email != null ? loaneeInfo.Email : DBNull.Value);
                    command.Parameters.AddWithValue("@phone", loaneeInfo.Phone != null ? loaneeInfo.Phone : DBNull.Value);
                    command.Parameters.AddWithValue("@notes", loaneeInfo.Note != null ? loaneeInfo.Note : DBNull.Value);
                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT last_insert_rowid()";
                    int lastInsertedID = Convert.ToInt32(command.ExecuteScalar());

                    DB.closeConnection(connection);

                    return lastInsertedID;

                }
            }

        }

        ///<summary>Gets the ids and names of loanees for a given userID. This is used primarily for populating
        ///the drop-down when documenting a new loan.</summary>
        public static List<Types.LoaneeName> GetNames(int userID) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT id, name FROM loanees WHERE iduser = @iduser";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@iduser", userID);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    List<Types.LoaneeName> loanees = new();
                    while (reader.Read()) {
                        Types.LoaneeName loanee = new();
                        loanee.Id = reader.GetInt32(0);
                        loanee.Name = reader.GetString(1);
                        loanees.Add(loanee);
                    }
                    DB.closeConnection(connection);
                    return loanees;
                }
            }
        }
        
    }

}