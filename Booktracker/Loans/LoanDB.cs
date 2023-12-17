using Microsoft.Data.Sqlite;

namespace bookTrackerApi.Loans {

    public static class LoanDB {

        public static int AddLoan(Types.NewLoan loanInfo, int userID) {

            using (SqliteConnection connection = DB.initiateConnection()) {

                string sql = "INSERT INTO loans (iduser, idbookList, idloanee, status, loanDate, returnDate, comment) VALUES (@iduser, @idbookList, @idloanee, @status, @loanDate, @returnDate, @comment)";
                using (SqliteCommand command = new(sql, connection)) {

                    command.Parameters.AddWithValue("@iduser", userID);
                    command.Parameters.AddWithValue("@idbookList", loanInfo.BookListID);
                    command.Parameters.AddWithValue("@idloanee", loanInfo.LoaneeID);
                    command.Parameters.AddWithValue("@status", "LOANED");
                    command.Parameters.AddWithValue("@loanDate", loanInfo.Date != null ? loanInfo.Date : DBNull.Value);
                    command.Parameters.AddWithValue("@returnDate", loanInfo.ReturnDate != null ? loanInfo.ReturnDate : DBNull.Value);
                    command.Parameters.AddWithValue("@comment", loanInfo.Comment != null ? loanInfo.Comment : DBNull.Value);
                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT last_insert_rowid()";
                    int lastInsertedID = Convert.ToInt32(command.ExecuteScalar());

                    DB.closeConnection(connection);

                    return lastInsertedID;
                }

            }

        }

    }

}