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

        ///<summary>Gets all loans for a given userID. Optional filters are available and will be described in the parameters.</summary>
        ///<param name="userID">The user ID who owns the loans that will be returned.</param>
        ///<param name="bookListID">OPTIONAL. If provided, only returns loans for that book.</param>
        ///<param name="status">OPTIONAL. If provided, only returns loans with that status.</param>
        ///<param name="loaneeID">OPTIONAL. If provided, only returns loan given to that loanee.</param>
        ///<returns>A list of loan info objects that should be used to build a table on the client-side.</returns>
        public static List<Types.BasicLoanInfo> GetLoans(int userID, int? bookListID, string? status, int? loaneeID) {

            using (SqliteConnection connection = DB.initiateConnection()) {

                string sql = @"
                    SELECT
                        loans.id,
                        loans.status,
                        loans.loanDate,
                        loans.returnDate,
                        loans.idbookList,
                        book_list2.title,
                        loans.idloanee,
                        loanees.name
                    FROM loans
                        JOIN book_list2 ON loans.idbookList = book_list2.iduser_books
                        JOIN loanees ON loans.idloanee = loanees.id
                    WHERE 
                        loans.iduser = @iduser
                ";

                if (bookListID != null) {
                    sql += " AND loans.idbookList = @idbookList";
                }

                if (status != null) {
                    sql += " AND loans.status = @status";
                }

                if (loaneeID != null) {
                    sql += " AND loans.idloanee = @idloanee";
                }

                using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                    command.Parameters.AddWithValue("@iduser", userID);

                    if (bookListID != null) {
                        command.Parameters.AddWithValue("@idbookList", bookListID);
                    }

                    if (status != null) {
                        command.Parameters.AddWithValue("@status", status);
                    }

                    if (loaneeID != null) {
                        command.Parameters.AddWithValue("@idloanee", loaneeID);
                    }

                    using (SqliteDataReader reader = command.ExecuteReader()) {

                        List<Types.BasicLoanInfo> loans = new();
                        while (reader.Read()) {
                            Types.BasicLoanInfo loan = new();
                            loan.Id = reader.GetInt32(0);
                            loan.Status = reader.GetString(1);
                            loan.LoanDate = reader.IsDBNull(2) ? null: reader.GetString(2);
                            loan.ReturnDate = reader.IsDBNull(3) ? null : reader.GetString(3);
                            loan.BookListID = reader.GetInt32(4);
                            loan.BookTitle = reader.GetString(5);
                            loan.LoaneeID = reader.GetInt32(6);
                            loan.LoaneeName = reader.GetString(7);
                            loans.Add(loan);
                        }
                        DB.closeConnection(connection);
                        return loans;
                    }

                }

            }

        }

        ///<summary>Deletes a loan.</summary>
        ///<param name="loanID">The ID of the loan that should be deleted.</param>
        public static void DeleteLoan(int loanID) {

            SqliteConnection connection = DB.initiateConnection();
            string sql = "DELETE FROM loans WHERE id = @id";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", loanID);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);

        }

        public static int GetOwner(int loanID) {

            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT iduser FROM loans WHERE id = @id";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@id", loanID);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    int owner = 0;
                    while (reader.Read()) {
                        owner = reader.GetInt32(0);
                    }
                    DB.closeConnection(connection);
                    return owner;
                }
            }

        }

        ///<summary>Updates the status, loanDate, returnDate, and comment for a given loan.</summary>
        ///<param name="updateInfo">The update info from the client. Things that don't change should be null.</param>
        public static void UpdateLoan(Types.LoanUpdate updateInfo, int id) {

            SqliteConnection connection = DB.initiateConnection();
            string sql = "UPDATE loans SET";
            if (updateInfo.Status != null) {
                sql += " status=@status,";
            }

            if (updateInfo.LoanDate != null) {
                sql += " loanDate=@loanDate,";
            }

            if (updateInfo.ReturnDate != null) {
                sql += " returnDate=@returnDate,";
            }

            if (updateInfo.Comment != null) {
                sql += " comment=@comment,";
            }
            sql += " WHERE id=@id";
            int location = sql.LastIndexOf(",");
            sql = sql.Remove(location, 1);
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            if (updateInfo.Status != null) {
                command.Parameters.AddWithValue("@status", updateInfo.Status);
            }

            if (updateInfo.LoanDate != null) {
                command.Parameters.AddWithValue("@loanDate", updateInfo.LoanDate);
            }

            if (updateInfo.ReturnDate != null) {
                command.Parameters.AddWithValue("@returnDate", updateInfo.ReturnDate);
            }

            if (updateInfo.Comment != null) {
                command.Parameters.AddWithValue("@comment", updateInfo.Comment);
            }
            command.ExecuteNonQuery();
            DB.closeConnection(connection);

        }




    }

}