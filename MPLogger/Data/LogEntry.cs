using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

/// <summary>
/// Represents a log entry with a message, route, and metadata such as ID and creation timestamp.
/// </summary>
/// <remarks>
/// This class is designed to be stored in a MongoDB database and uses BSON attributes for serialization.
/// </remarks>
/// <param name="message">The log message describing the event or action.</param>
/// <param name="route">The route or endpoint associated with the log entry.</param>
[BsonDiscriminator(RootClass = true)]
[BsonKnownTypes(typeof(LogEntry))]
public class LogEntry(string message, string route)
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Message { get; set; } = message;
    public string Route { get; set; } = route;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
