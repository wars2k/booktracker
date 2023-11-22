using Microsoft.Data.Sqlite;

namespace bookTrackerApi {

    public static class CollectionsDB {

        public static List<CollectionTypes.CollectionNames> getCollectionsNames(SessionInfo session) {

            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT * FROM collections WHERE userID = @userID";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@userID", session.AssociatedID);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    List<CollectionTypes.CollectionNames> collectionNames = new List<CollectionTypes.CollectionNames>();
                    while (reader.Read()) {
                        CollectionTypes.CollectionNames collection = new CollectionTypes.CollectionNames();
                        collection.CollectionID = reader.GetInt32(0);
                        collection.CollectionName = reader.GetString(1);
                        collectionNames.Add(collection);
                    }
                    DB.closeConnection(connection);
                    return collectionNames;
                }
            }
            

        }

        public static List<CollectionTypes.CollectionMetadata> getCollectionMetadata(SessionInfo session) {

            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT * FROM collections WHERE userID = @userID";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@userID", session.AssociatedID);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    List<CollectionTypes.CollectionMetadata> collectionMetadata = new List<CollectionTypes.CollectionMetadata>();
                    while (reader.Read()) {
                        CollectionTypes.CollectionMetadata collection = new CollectionTypes.CollectionMetadata();
                        collection.CollectionID = reader.GetInt32(0);
                        collection.Name = reader.GetString(1);
                        collection.Description = reader.IsDBNull(2) ? null: reader.GetString(2);
                        collection.CoverImage = reader.IsDBNull(3) ? null: reader.GetString(3);
                        collection.OwnerID = reader.IsDBNull(4) ? null: reader.GetInt32(4);
                        collection.createdDate = reader.IsDBNull(5) ? null: reader.GetString(5);
                        collectionMetadata.Add(collection);
                    }
                    DB.closeConnection(connection);
                    return collectionMetadata;
                }
            }

        }

        public static List<int> getCollectionBookIDs(int? collectionID) {

            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT * FROM collection_entries WHERE idcollection = @collectionID";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@collectionID", collectionID);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    List<int> collectionBooksIDs = new List<int>();
                    while (reader.Read()) {
                        int collectionEntry = reader.GetInt32(2);
                        collectionBooksIDs.Add(collectionEntry);
                    }
                    DB.closeConnection(connection);
                    return collectionBooksIDs;
                }
            }

        }

        public static void createNew(APITypes.newCollectionRequestBody info, SessionInfo sessionInfo) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "INSERT INTO collections (collection_name, collection_description, collection_cover_image, userID, dateTime) VALUES (@name, @description, @coverImage, @userID, @dateTime)";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@name", info.Name);
            command.Parameters.AddWithValue("@description", info.Description != null ? info.Description : DBNull.Value);
            command.Parameters.AddWithValue("@coverImage", info.CoverImage != null ? info.CoverImage : DBNull.Value);
            command.Parameters.AddWithValue("@userID", sessionInfo.AssociatedID);
            command.Parameters.AddWithValue("@dateTime", DateTime.Now);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);
        }

        public static CollectionTypes.Collection getById(int id) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT * FROM collections WHERE idcollection = @id";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@id", id);
                CollectionTypes.Collection collectionMain = new CollectionTypes.Collection();
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        CollectionTypes.Collection collection = new CollectionTypes.Collection();
                        collection.CollectionID = id;
                        collection.Name = reader.GetString(1);
                        collection.Description = reader.IsDBNull(2) ? null: reader.GetString(2);
                        collection.CoverImage = reader.IsDBNull(3) ? null: reader.GetString(3);
                        collection.OwnerID = reader.IsDBNull(4) ? null: reader.GetInt32(4);
                        collection.createdDate = reader.IsDBNull(5) ? null: reader.GetString(5);
                        collectionMain = collection;
                        
                    }
                    DB.closeConnection(connection);
                    return collectionMain;
                }
            }
        }

        public static void deleteCollection(int id) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "DELETE FROM collections WHERE idcollection = @id";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);
        }

        public static void update(APITypes.newCollectionRequestBody updatedInfo, CollectionTypes.Collection currentInfo) {
            if (updatedInfo.Name == null) {
                updatedInfo.Name = currentInfo.Name;
            }
            if (updatedInfo.Description == null) {
                updatedInfo.Description = currentInfo.Description;
            }
            if (updatedInfo.CoverImage == null) {
                updatedInfo.CoverImage = currentInfo.CoverImage;
            }
            SqliteConnection connection = DB.initiateConnection();
            string sql = "UPDATE collections SET collection_name = @name, collection_description = @description, collection_cover_image = @coverImage WHERE idcollection = @id";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", currentInfo.CollectionID);
            command.Parameters.AddWithValue("@name", updatedInfo.Name);
            command.Parameters.AddWithValue("@description", updatedInfo.Description != null ? updatedInfo.Description : DBNull.Value);
            command.Parameters.AddWithValue("@coverImage", updatedInfo.CoverImage != null ? updatedInfo.CoverImage : DBNull.Value);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);
        }

        public static void addBook(int id, int bookId) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "INSERT INTO collection_entries (idcollection, iduser_book, dateTime) VALUES (@id, @bookId, @dateTime)";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@bookId", bookId);
            command.Parameters.AddWithValue("@dateTime", DateTime.Now);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);
        }

        public static Boolean checkBookId(int id, int userId) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT COUNT(*) FROM user_books WHERE iduser_books = @id AND iduser = @userId";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@userId", userId);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    if (reader.Read()) {
                        int? count = reader.GetInt32(0);
                        return count != 0;
                    }
                }
            }
            return false;
        }

        public static void deleteBook(int id, int bookId) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "DELETE FROM collection_entries WHERE idcollection = @id AND iduser_book = @bookId";
            SqliteCommand command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@bookId", bookId);
            command.ExecuteNonQuery();
            DB.closeConnection(connection);
        }

        public static List<int> getIDsForBookListID(int id) {
            SqliteConnection connection = DB.initiateConnection();
            string sql = "SELECT idcollection FROM collection_entries WHERE iduser_book = @id";
            using (SqliteCommand command = new SqliteCommand(sql, connection)) {
                command.Parameters.AddWithValue("@id", id);
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    List<int> list = new();
                    while (reader.Read()) {
                        int collectionID = reader.GetInt32(0);
                        list.Add(collectionID);
                        
                    }
                    DB.closeConnection(connection);
                    return list;
                }
            }

        }



    }

    public static class CollectionTypes {

        public class CollectionNames {
            public int? CollectionID { get; set; }
            public string? CollectionName { get; set; }
        }

        public class CollectionMetadata {
            public int? CollectionID { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string? CoverImage { get; set; }
            public int? OwnerID { get; set; }
            public string? createdDate { get; set; }
        }

        public class Collection {
            public int? CollectionID { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string? CoverImage { get; set; }
            public int? OwnerID { get; set; }
            public string? createdDate { get; set; }
            public List<int>? listOfBookID { get; set; }
        }

    }

}