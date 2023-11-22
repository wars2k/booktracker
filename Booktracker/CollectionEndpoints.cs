using Newtonsoft.Json;
using System.Text;

namespace bookTrackerApi {

    public static class CollectionEndpoints {

        public static void configureCollectionEndpoints(WebApplication app) {

            //Get a list of arrays with various data depending on what "include" parameter is set to.
            app.MapGet("/api/collections", (String include, String sessionKey, HttpContext context, String? bookListID) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "collection_view", null, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                if (include != "name" && include != "metadata" && include != "all") {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_paramter, "collection_view", currentSession, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                if (include == "name") {
                    if (bookListID != null) {
                        List<int> collectionIDs = CollectionsDB.getIDsForBookListID(Int32.Parse(bookListID));
                        return Results.Ok(collectionIDs);
                    }
                    List<CollectionTypes.CollectionNames> collections = CollectionsDB.getCollectionsNames(currentSession);
                    return Results.Ok(collections);
                } else if (include == "metadata") {
                    List<CollectionTypes.CollectionMetadata> collections = CollectionsDB.getCollectionMetadata(currentSession);
                    return Results.Ok(collections);
                } else {
                    List<CollectionTypes.CollectionMetadata> collectionsMetadata = CollectionsDB.getCollectionMetadata(currentSession);
                    List<CollectionTypes.Collection> collections = new List<CollectionTypes.Collection>();
                    foreach (CollectionTypes.CollectionMetadata OneCollectionMetadata in collectionsMetadata) {
                        CollectionTypes.Collection collection = new CollectionTypes.Collection();
                        collection.CollectionID = OneCollectionMetadata.CollectionID;
                        collection.CoverImage = OneCollectionMetadata.CoverImage;
                        collection.createdDate = OneCollectionMetadata.createdDate;
                        collection.Description = OneCollectionMetadata.Description;
                        collection.Name = OneCollectionMetadata.Name;
                        collection.OwnerID = OneCollectionMetadata.OwnerID;
                        collection.listOfBookID = CollectionsDB.getCollectionBookIDs(OneCollectionMetadata.CollectionID);
                        collections.Add(collection);
                    }
                    return Results.Ok(collections);
                }

            })
            .Produces<List<CollectionTypes.Collection>>(StatusCodes.Status200OK)
            .Produces<List<CollectionTypes.CollectionMetadata>>(StatusCodes.Status200OK)
            .Produces<List<CollectionTypes.CollectionNames>>(StatusCodes.Status200OK)
            .Produces<ErrorMessage>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Retrieves a list of all collections.",
                Description = "Accepts three different options for include ('name', 'metadata' and 'all'). The output changes based on this parameter."
            });

            app.MapPost("/api/collections/new", async (String sessionKey, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "collection_create", null, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<APITypes.newCollectionRequestBody>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.missing_request_body, "collection_create", currentSession, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                if (payload.Name == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_request_body, "collection_create", currentSession, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                JsonLog.writeLog($"New collection '{payload.Name}' created.", "INFO", "collection_create", currentSession, remoteIp);
                CollectionsDB.createNew(payload, currentSession);
                return Results.Ok();
            })
            .Accepts<APITypes.newCollectionRequestBody>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorMessage>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Adds a new collection."
            });

            app.MapGet("/api/collections/{id}", (String sessionKey, int id, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "collection_viewOne", null, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                CollectionTypes.Collection collection = CollectionsDB.getById(id);
                collection.listOfBookID = CollectionsDB.getCollectionBookIDs(id);
                if (int.Parse(currentSession.AssociatedID) == collection.OwnerID) {
                    return Results.Ok(collection);
                } else {
                    JsonLog.writeLog("Unauthorized attempt to access another user's Collection by ID.", "WARNING", "collection_viewOne", currentSession, remoteIp);
                    return Results.Unauthorized();
                }
            })
            .Produces<CollectionTypes.Collection>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ErrorMessage>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Retrieves a collection by ID."
            });

            app.MapDelete("/api/collections/{id}", (String sessionKey, int id, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "collection_delete", null, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                CollectionTypes.Collection collection = CollectionsDB.getById(id);
                if (int.Parse(currentSession.AssociatedID) == collection.OwnerID) {
                    CollectionsDB.deleteCollection(id);
                    JsonLog.writeLog($"Collection ID '{id}' deleted.", "INFO", "collection_delete", currentSession, remoteIp);
                    return Results.Ok("Collection deleted.");
                } else {
                    JsonLog.writeLog("Unauthorized attempt to delete another user's collection.", "WARNING", "collection_delete", currentSession, remoteIp);
                    return Results.Unauthorized();
                }
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ErrorMessage>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Deletes a collection by ID."
            });

            app.MapPut("/api/collections/{id}", async (String sessionKey, int id, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "collection_delete", null, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<APITypes.newCollectionRequestBody>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_request_body, "collection_update", currentSession, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                CollectionTypes.Collection currentInfo = CollectionsDB.getById(id);
                if (int.Parse(currentSession.AssociatedID) == currentInfo.OwnerID) {
                    JsonLog.writeLog($"Collection ID '{id}' metadata updated.", "INFO", "collection_update", currentSession, remoteIp);
                    CollectionsDB.update(payload, currentInfo);
                    return Results.Ok();
                } else {
                    JsonLog.writeLog("Unauthorized attempt to update another user's collection metadata.", "WARNING", "collection_update", currentSession, remoteIp);
                    return Results.Unauthorized();
                }
                
            })
            .Accepts<APITypes.newCollectionRequestBody>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ErrorMessage>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Updates a collection by ID."
            });

            app.MapPost("/api/collections/{id}/add/{bookId}", (String sessionKey, int id, int bookId, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "collection_add", null, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                CollectionTypes.Collection currentInfo = CollectionsDB.getById(id);
                currentInfo.listOfBookID = CollectionsDB.getCollectionBookIDs(id);
                Boolean isBookAlreadyAdded = currentInfo.listOfBookID.Contains(bookId);
                if (int.Parse(currentSession.AssociatedID) == currentInfo.OwnerID) {
                    Boolean isValidBookId = CollectionsDB.checkBookId(bookId, int.Parse(currentSession.AssociatedID));
                    if (isValidBookId && !isBookAlreadyAdded) {
                        CollectionsDB.addBook(id, bookId);
                        return Results.Ok();
                    }
                    JsonLog.writeLog("Unsuccessful attempt to add a book to a collection. Attempt likely failed due to invalid or book ID or book already being part of the collection.", "ERROR", "collection_add", currentSession, remoteIp);
                    return Results.BadRequest(isValidBookId + " " + currentSession.AssociatedID + " " + bookId);
                } else {
                    JsonLog.writeLog("Unauthorized attempt to add a book to another user's collection.", "WARNING", "collection_add", currentSession, remoteIp);
                    return Results.Unauthorized();
                }
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Adds a given BookID to a collection by ID."
            });

            app.MapPost("/api/collection/{id}/bulkAdd", async (String sessionKey, int id, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "collection_bulkAdd", null, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<APITypes.bulkCollectionBody>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_request_body, "collection_bulkAdd", currentSession, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                CollectionTypes.Collection currentInfo = CollectionsDB.getById(id);
                currentInfo.listOfBookID = CollectionsDB.getCollectionBookIDs(id);
                if (int.Parse(currentSession.AssociatedID) != currentInfo.OwnerID) {
                    JsonLog.writeLog("Unauthorized attempt to bulk add books to another user's collection.", "WARNING", "collection_bulkAdd", currentSession, remoteIp);
                    return Results.Unauthorized();
                }
                for (int i = 0; i < payload.Data.Count; i++) {
                    Boolean isBookAlreadyAdded = currentInfo.listOfBookID.Contains(payload.Data[i]);
                    Boolean isValidBookId = CollectionsDB.checkBookId(payload.Data[i], int.Parse(currentSession.AssociatedID));
                    if (isValidBookId && !isBookAlreadyAdded) {
                        CollectionsDB.addBook(id, payload.Data[i]);
                    }
                }
                return Results.Ok();
            })
            .Accepts<APITypes.bulkCollectionBody>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Adds a given array of BookIDs to a collection by ID."
            });


            app.MapDelete("/api/collections/{id}/remove/{bookId}", (String sessionKey, int id, int bookId, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "collection_deleteBook", null, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                CollectionTypes.Collection currentInfo = CollectionsDB.getById(id);
                currentInfo.listOfBookID = CollectionsDB.getCollectionBookIDs(id);
                Boolean isBookAlreadyAdded = currentInfo.listOfBookID.Contains(bookId);
                if (int.Parse(currentSession.AssociatedID) == currentInfo.OwnerID) {
                    Boolean isValidBookId = CollectionsDB.checkBookId(bookId, int.Parse(currentSession.AssociatedID));
                    if (isValidBookId && isBookAlreadyAdded) {
                        JsonLog.writeLog($"Book ID {bookId} deleted from Collection ID {id}.", "INFO", "collection_deleteBook", currentSession, remoteIp);
                        CollectionsDB.deleteBook(id, bookId);
                        return Results.Ok();
                    }
                    JsonLog.writeLog("Unsuccessful attempt to delete a book from a collection. Attempt likely failed due to invalid or book ID or book already being part of the collection.", "ERROR", "collection_deleteBook", currentSession, remoteIp);
                    return Results.BadRequest(isValidBookId + " " + currentSession.AssociatedID + " " + bookId);
                } else {
                    JsonLog.writeLog("Unauthorized attempt to delete a book from another user's collection.", "WARNING", "collection_deleteBook", currentSession, remoteIp);
                    return Results.Unauthorized();
                }
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ErrorMessage>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Removes a given BookID from a collection by ID."
            });

            app.MapDelete("/api/collection/{id}/bulkAdd", async (String sessionKey, int id, HttpContext context) => {
                string? remoteIp = context.Connection.RemoteIpAddress?.ToString();
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_sessionKey, "collection_bulkDeleteBook", null, remoteIp); 
                    return Results.BadRequest(errorMessage);
                }
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<APITypes.bulkCollectionBody>(requestBody);
                if (payload == null) {
                    ErrorMessage errorMessage = JsonLog.logAndCreateErrorMessage(ErrorMessages.invalid_request_body, "collection_bulkDeleteBook", currentSession, remoteIp);
                    return Results.BadRequest(errorMessage);
                }
                CollectionTypes.Collection currentInfo = CollectionsDB.getById(id);
                currentInfo.listOfBookID = CollectionsDB.getCollectionBookIDs(id);
                if (int.Parse(currentSession.AssociatedID) != currentInfo.OwnerID) {
                    JsonLog.writeLog("Unauthorized attempt to bulk delete books from another user's collection.", "WARNING", "collection_bulkDeleteBook", currentSession, remoteIp);
                    return Results.Unauthorized();
                }
                for (int i = 0; i < payload.Data.Count; i++) {
                    Boolean isBookAlreadyAdded = currentInfo.listOfBookID.Contains(payload.Data[i]);
                    Boolean isValidBookId = CollectionsDB.checkBookId(payload.Data[i], int.Parse(currentSession.AssociatedID));
                    if (isValidBookId && isBookAlreadyAdded) {
                        CollectionsDB.deleteBook(id, payload.Data[i]);
                    }
                }
                JsonLog.writeLog($"{payload.Data.Count} books have been deleted from Collection ID {id}.", "INFO", "collection_bulkDeleteBook", currentSession, remoteIp);
                return Results.Ok();
            })
            .Accepts<APITypes.bulkCollectionBody>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ErrorMessage>(StatusCodes.Status400BadRequest)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Deketes a given array of BookIDs from a collection by ID."
            });


        }

    }

    public static class APITypes {

        public class newCollectionRequestBody {

            public string? Name { get; set; }
            public string? Description { get; set; }
            public string? CoverImage { get; set; }
        }

        public class bulkCollectionBody {
            public List<int>? Data { get; set ;}
        }

        public class ErrorMessage {
            public string? Code { get; set; }
            public string? Message { get; set; }
            public string? Details { get; set; }
        }

        

    }
}