using Newtonsoft.Json;
using System.Text;

namespace bookTrackerApi {

    public static class CollectionEndpoints {

        public static void configureCollectionEndpoints(WebApplication app) {

            //Get a list of arrays with various data depending on what "include" parameter is set to.
            app.MapGet("/api/collections", (String include, String sessionKey) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest("Invalid session key");
                }
                if (include != "name" && include != "metadata" && include != "all") {
                    return Results.BadRequest($"Invalid parameter 'include': {include}");
                }
                if (include == "name") {
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
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Retrieves a list of all collections.",
                Description = "Accepts three different options for include ('name', 'metadata' and 'all'). The output changes based on this parameter."
            });

            app.MapPost("/api/collections/new", async (String sessionKey, HttpContext context) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest("Invalid session key");
                }
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<APITypes.newCollectionRequestBody>(requestBody);
                if (payload == null) {
                    return Results.BadRequest("incorrect format for body");
                }
                if (payload.Name == null) {
                    return Results.BadRequest("name required to create new collection");
                }
                CollectionsDB.createNew(payload, currentSession);
                return Results.Ok();
            })
            .Accepts<APITypes.newCollectionRequestBody>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Adds a new collection."
            });

            app.MapGet("/api/collections/{id}", (String sessionKey, int id) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest("Invalid session key");
                }
                CollectionTypes.Collection collection = CollectionsDB.getById(id);
                collection.listOfBookID = CollectionsDB.getCollectionBookIDs(id);
                if (int.Parse(currentSession.AssociatedID) == collection.OwnerID) {
                    return Results.Ok(collection);
                } else {
                    return Results.Unauthorized();
                }
            })
            .Produces<CollectionTypes.Collection>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Retrieves a collection by ID."
            });

            app.MapDelete("/api/collections/{id}", (String sessionKey, int id) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest("Invalid session key");
                }
                CollectionTypes.Collection collection = CollectionsDB.getById(id);
                if (int.Parse(currentSession.AssociatedID) == collection.OwnerID) {
                    CollectionsDB.deleteCollection(id);
                    return Results.Ok("Collection deleted.");
                } else {
                    return Results.Unauthorized();
                }
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Deletes a collection by ID."
            });

            app.MapPut("/api/collections/{id}", async (String sessionKey, int id, HttpContext context) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest("Invalid session key");
                }
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<APITypes.newCollectionRequestBody>(requestBody);
                if (payload == null) {
                    return Results.BadRequest("incorrect format for body");
                }
                CollectionTypes.Collection currentInfo = CollectionsDB.getById(id);
                if (int.Parse(currentSession.AssociatedID) == currentInfo.OwnerID) {
                    CollectionsDB.update(payload, currentInfo);
                    return Results.Ok();
                } else {
                    return Results.Unauthorized();
                }
                
            })
            .Accepts<APITypes.newCollectionRequestBody>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Updates a collection by ID."
            });

            app.MapPost("/api/collections/{id}/add/{bookId}", (String sessionKey, int id, int bookId) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest("Invalid session key");
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
                    return Results.BadRequest(isValidBookId + " " + currentSession.AssociatedID + " " + bookId);
                } else {
                    return Results.Unauthorized();
                }
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Adds a given BookID to a collection by ID."
            });

            app.MapDelete("/api/collections/{id}/remove/{bookId}", (String sessionKey, int id, int bookId) => {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null) {
                    return Results.BadRequest("Invalid session key");
                }
                CollectionTypes.Collection currentInfo = CollectionsDB.getById(id);
                currentInfo.listOfBookID = CollectionsDB.getCollectionBookIDs(id);
                Boolean isBookAlreadyAdded = currentInfo.listOfBookID.Contains(bookId);
                if (int.Parse(currentSession.AssociatedID) == currentInfo.OwnerID) {
                    Boolean isValidBookId = CollectionsDB.checkBookId(bookId, int.Parse(currentSession.AssociatedID));
                    if (isValidBookId && isBookAlreadyAdded) {
                        CollectionsDB.deleteBook(id, bookId);
                        return Results.Ok();
                    }
                    return Results.BadRequest(isValidBookId + " " + currentSession.AssociatedID + " " + bookId);
                } else {
                    return Results.Unauthorized();
                }
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Removes a given BookID from a collection by ID."
            });


        }

    }

    public static class APITypes {

        public class newCollectionRequestBody {

            public string? Name { get; set; }
            public string? Description { get; set; }
            public string? CoverImage { get; set; }
        }

    }
}