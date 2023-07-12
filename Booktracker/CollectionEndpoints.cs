using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace bookTrackerApi;

public static class CollectionEndpoints
{
    public static void configureCollectionEndpoints(WebApplication app)
    {
        //Get a list of arrays with various data depending on what "include" parameter is set to.
        app.MapGet("/api/collections", (string include, string sessionKey) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                {
                    return Results.BadRequest("Invalid session key");
                }

                if (include != "name" && include != "metadata" && include != "all")
                {
                    return Results.BadRequest($"Invalid parameter 'include': {include}");
                }

                if (include == "name")
                {
                    List<CollectionTypes.CollectionNames> collections =
                        new CollectionsDB().getCollectionsNames(currentSession);
                    return Results.Ok(collections);
                }

                if (include == "metadata")
                {
                    List<CollectionTypes.CollectionMetadata> collections =
                        new CollectionsDB().getCollectionMetadata(currentSession);
                    return Results.Ok(collections);
                }
                else
                {
                    List<CollectionTypes.CollectionMetadata> collectionsMetadata =
                        new CollectionsDB().getCollectionMetadata(currentSession);
                    List<CollectionTypes.Collection> collections = new List<CollectionTypes.Collection>();
                    foreach (CollectionTypes.CollectionMetadata OneCollectionMetadata in collectionsMetadata)
                    {
                        CollectionTypes.Collection collection = new CollectionTypes.Collection();
                        collection.CollectionID = OneCollectionMetadata.CollectionID;
                        collection.CoverImage = OneCollectionMetadata.CoverImage;
                        collection.createdDate = OneCollectionMetadata.createdDate;
                        collection.Description = OneCollectionMetadata.Description;
                        collection.Name = OneCollectionMetadata.Name;
                        collection.OwnerID = OneCollectionMetadata.OwnerID;
                        collection.listOfBookID =
                            new CollectionsDB().getCollectionBookIDs(OneCollectionMetadata.CollectionID);
                        collections.Add(collection);
                    }

                    return Results.Ok(collections);
                }
            })
            .Produces<List<CollectionTypes.Collection>>()
            .Produces<List<CollectionTypes.CollectionMetadata>>()
            .Produces<List<CollectionTypes.CollectionNames>>()
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Retrieves a list of all collections.",
                Description =
                    "Accepts three different options for include ('name', 'metadata' and 'all'). The output changes based on this parameter."
            });

        app.MapPost("/api/collections/new", async (string sessionKey, HttpContext context) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                {
                    return Results.BadRequest("Invalid session key");
                }

                using StreamReader reader = new StreamReader(context.Request.Body);
                string requestBody = await reader.ReadToEndAsync();
                APITypes.newCollectionRequestBody? payload = JsonConvert.DeserializeObject<APITypes.newCollectionRequestBody>(requestBody);
                if (payload == null)
                {
                    return Results.BadRequest("incorrect format for body");
                }

                if (payload.Name == null)
                {
                    return Results.BadRequest("name required to create new collection");
                }

                new CollectionsDB().createNew(payload, currentSession);
                return Results.Ok();
            })
            .Accepts<APITypes.newCollectionRequestBody>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Adds a new collection."
            });

        app.MapGet("/api/collections/{id}", (string sessionKey, int id) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                {
                    return Results.BadRequest("Invalid session key");
                }

                CollectionTypes.Collection collection = new CollectionsDB().getById(id);
                collection.listOfBookID = new CollectionsDB().getCollectionBookIDs(id);
                if (int.Parse(currentSession.AssociatedID) == collection.OwnerID)
                {
                    return Results.Ok(collection);
                }

                return Results.Unauthorized();
            })
            .Produces<CollectionTypes.Collection>()
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Retrieves a collection by ID."
            });

        app.MapDelete("/api/collections/{id}", (string sessionKey, int id) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                {
                    return Results.BadRequest("Invalid session key");
                }

                CollectionTypes.Collection collection = new CollectionsDB().getById(id);
                if (int.Parse(currentSession.AssociatedID) == collection.OwnerID)
                {
                    new CollectionsDB().deleteCollection(id);
                    return Results.Ok("Collection deleted.");
                }

                return Results.Unauthorized();
            })
            .Produces<string>()
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Deletes a collection by ID."
            });

        app.MapPut("/api/collections/{id}", async (string sessionKey, int id, HttpContext context) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                {
                    return Results.BadRequest("Invalid session key");
                }

                using StreamReader reader = new StreamReader(context.Request.Body);
                string requestBody = await reader.ReadToEndAsync();
                APITypes.newCollectionRequestBody? payload = JsonConvert.DeserializeObject<APITypes.newCollectionRequestBody>(requestBody);
                if (payload == null)
                {
                    return Results.BadRequest("incorrect format for body");
                }

                CollectionTypes.Collection currentInfo = new CollectionsDB().getById(id);
                if (int.Parse(currentSession.AssociatedID) == currentInfo.OwnerID)
                {
                    new CollectionsDB().update(payload, currentInfo);
                    return Results.Ok();
                }

                return Results.Unauthorized();
            })
            .Accepts<APITypes.newCollectionRequestBody>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Updates a collection by ID."
            });

        app.MapPost("/api/collections/{id}/add/{bookId}", (string sessionKey, int id, int bookId) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                {
                    return Results.BadRequest("Invalid session key");
                }

                CollectionTypes.Collection currentInfo = new CollectionsDB().getById(id);
                currentInfo.listOfBookID = new CollectionsDB().getCollectionBookIDs(id);
                bool isBookAlreadyAdded = currentInfo.listOfBookID.Contains(bookId);
                if (int.Parse(currentSession.AssociatedID) == currentInfo.OwnerID)
                {
                    bool isValidBookId =
                        new CollectionsDB().checkBookId(bookId, int.Parse(currentSession.AssociatedID));
                    if (isValidBookId && !isBookAlreadyAdded)
                    {
                        new CollectionsDB().addBook(id, bookId);
                        return Results.Ok();
                    }

                    return Results.BadRequest(isValidBookId + " " + currentSession.AssociatedID + " " + bookId);
                }

                return Results.Unauthorized();
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Collections")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Adds a given BookID to a collection by ID."
            });

        app.MapPost("/api/collection/{id}/bulkAdd", async (string sessionKey, int id, HttpContext context) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                {
                    return Results.BadRequest("Invalid session key");
                }

                using StreamReader reader = new StreamReader(context.Request.Body);
                string requestBody = await reader.ReadToEndAsync();
                APITypes.bulkCollectionBody? payload = JsonConvert.DeserializeObject<APITypes.bulkCollectionBody>(requestBody);
                if (payload == null)
                {
                    return Results.BadRequest("incorrect format for body");
                }

                CollectionTypes.Collection currentInfo = new CollectionsDB().getById(id);
                currentInfo.listOfBookID = new CollectionsDB().getCollectionBookIDs(id);
                if (int.Parse(currentSession.AssociatedID) != currentInfo.OwnerID)
                {
                    return Results.Unauthorized();
                }

                for (int i = 0; i < payload.Data.Count; i++)
                {
                    bool isBookAlreadyAdded = currentInfo.listOfBookID.Contains(payload.Data[i]);
                    bool isValidBookId =
                        new CollectionsDB().checkBookId(payload.Data[i], int.Parse(currentSession.AssociatedID));
                    if (isValidBookId && !isBookAlreadyAdded)
                    {
                        new CollectionsDB().addBook(id, payload.Data[i]);
                    }
                }

                return Results.Ok();
            })
            .Accepts<APITypes.bulkCollectionBody>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Collections")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Adds a given array of BookIDs to a collection by ID."
            });


        app.MapDelete("/api/collections/{id}/remove/{bookId}", (string sessionKey, int id, int bookId) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                {
                    return Results.BadRequest("Invalid session key");
                }

                CollectionTypes.Collection currentInfo = new CollectionsDB().getById(id);
                currentInfo.listOfBookID = new CollectionsDB().getCollectionBookIDs(id);
                bool isBookAlreadyAdded = currentInfo.listOfBookID.Contains(bookId);
                if (int.Parse(currentSession.AssociatedID) == currentInfo.OwnerID)
                {
                    bool isValidBookId =
                        new CollectionsDB().checkBookId(bookId, int.Parse(currentSession.AssociatedID));
                    if (isValidBookId && isBookAlreadyAdded)
                    {
                        new CollectionsDB().deleteBook(id, bookId);
                        return Results.Ok();
                    }

                    return Results.BadRequest(isValidBookId + " " + currentSession.AssociatedID + " " + bookId);
                }

                return Results.Unauthorized();
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .WithTags("Collections")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Removes a given BookID from a collection by ID."
            });

        app.MapDelete("/api/collection/{id}/bulkAdd", async (string sessionKey, int id, HttpContext context) =>
            {
                SessionInfo? currentSession = Program.Sessions.Find(s => s.Session == sessionKey);
                if (currentSession == null)
                {
                    return Results.BadRequest("Invalid session key");
                }

                using StreamReader reader = new StreamReader(context.Request.Body);
                string requestBody = await reader.ReadToEndAsync();
                APITypes.bulkCollectionBody? payload = JsonConvert.DeserializeObject<APITypes.bulkCollectionBody>(requestBody);
                if (payload == null)
                {
                    return Results.BadRequest("incorrect format for body");
                }

                CollectionTypes.Collection currentInfo = new CollectionsDB().getById(id);
                currentInfo.listOfBookID = new CollectionsDB().getCollectionBookIDs(id);
                if (int.Parse(currentSession.AssociatedID) != currentInfo.OwnerID)
                {
                    return Results.Unauthorized();
                }

                for (int i = 0; i < payload.Data.Count; i++)
                {
                    bool isBookAlreadyAdded = currentInfo.listOfBookID.Contains(payload.Data[i]);
                    bool isValidBookId =
                        new CollectionsDB().checkBookId(payload.Data[i], int.Parse(currentSession.AssociatedID));
                    if (isValidBookId && isBookAlreadyAdded)
                    {
                        new CollectionsDB().deleteBook(id, payload.Data[i]);
                    }
                }

                return Results.Ok();
            })
            .Accepts<APITypes.bulkCollectionBody>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Collections")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Deketes a given array of BookIDs from a collection by ID."
            });
    }
}

public static class APITypes
{
    public class newCollectionRequestBody
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CoverImage { get; set; }
    }

    public class bulkCollectionBody
    {
        public List<int>? Data { get; set; }
    }
}