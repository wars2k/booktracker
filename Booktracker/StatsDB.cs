using Microsoft.Data.Sqlite;

namespace bookTrackerApi {

    public static class StatsDB {

        public static StatsTypes.StatusCounts GetStatusCounts(SessionInfo session) {

            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT COUNT(CASE WHEN iduser = @id AND status = 'UNASSIGNED')AS count_unassigned, COUNT(CASE WHEN iduser = @id AND status = 'READING')AS count_reading, COUNT(CASE WHEN iduser = @id AND status = 'UP NEXT')AS count_next, COUNT(CASE WHEN iduser = @id AND status = 'WISHLIST')AS count_wishlist, COUNT(CASE WHEN iduser = @id AND status = 'FINISHED')AS count_finished FROM user_books";
            Console.WriteLine(sql);
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@id", session.AssociatedID);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    StatsTypes.StatusCounts statusCounts = new StatsTypes.StatusCounts();
                    while (reader.Read()) {
                        statusCounts.Unassigned = reader.GetInt32(0);
                        statusCounts.Reading = reader.GetInt32(1);
                        statusCounts.UpNext = reader.GetInt32(2);
                        statusCounts.Wishlist = reader.GetInt32(3);
                        statusCounts.Finished = reader.GetInt32(4);
                    }
                    DB.closeConnection(connection);
                    return statusCounts;
                }
            }

        }

        public static List<StatsTypes.MonthlyFinishedBooks> GetBooksFinishedPerMonth(SessionInfo session) {
            List<StatsTypes.MonthlyFinishedBooks> monthlyFinishedBooks = new List<StatsTypes.MonthlyFinishedBooks>();
            
            return monthlyFinishedBooks;
        }

    }

    public static class StatsTypes {

        public class StatusCounts {
            public int? Unassigned { get; set; }
            public int? Reading { get; set; }
            public int? UpNext { get; set; }
            public int? Wishlist { get; set; }
            public int? Finished { get; set; }
        }

        public class MonthlyFinishedBooks {
            public int? Month { get; set; }
            public int? Count { get; set; }
        }

    }
}