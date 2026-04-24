// FitnessCenterr.Migrator – One-time migration: MySQL → MongoDB + Neo4j

using FitnessCenterr.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Neo4j.Driver;

// ── Konfiguration ─────────────────────────────────────────────────────────────
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())  // ← Fix linje 12
    .AddJsonFile("appsettings.json")
    .Build();

var mysqlConn   = config.GetConnectionString("DefaultConnection")!;
var mongoConn   = config["MongoDB:ConnectionString"]!;
var mongoDbName = config["MongoDB:Database"]!;
var neo4jUri    = config["Neo4j:Uri"]!;
var neo4jUser   = config["Neo4j:Username"]!;
var neo4jPass   = config["Neo4j:Password"]!;

// ── Hent data fra MySQL ───────────────────────────────────────────────────────
Console.WriteLine("Henter data fra MySQL...");

var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseMySql(mysqlConn, new MySqlServerVersion(new Version(8, 0, 0)))  // ← Fix linje 27
    .Options;

using var db = new AppDbContext(dbOptions);

var trainers      = await db.Trainers.ToListAsync();
var members       = await db.Members.Include(m => m.Trainer).ToListAsync();
var subscriptions = await db.Subscriptions.ToListAsync();
var memberships   = await db.Memberships.Include(m => m.Member).Include(m => m.Subscription).ToListAsync();
var classes       = await db.Classes.Include(c => c.Trainer).ToListAsync();
var bookings      = await db.ClassBookings.Include(b => b.Member).Include(b => b.Class).ToListAsync();

Console.WriteLine($"  Trainers: {trainers.Count}, Members: {members.Count}, Classes: {classes.Count}");
Console.WriteLine($"  Subscriptions: {subscriptions.Count}, Memberships: {memberships.Count}, Bookings: {bookings.Count}");

// ── MongoDB ───────────────────────────────────────────────────────────────────
Console.WriteLine("\nMigrerer til MongoDB...");

var mongoClient = new MongoClient(mongoConn);  // ← Fix: ingen cast
var mongoDB     = mongoClient.GetDatabase(mongoDbName);

await mongoDB.DropCollectionAsync("members");
await mongoDB.DropCollectionAsync("classes");
await mongoDB.DropCollectionAsync("subscriptions");

var membersCol = mongoDB.GetCollection<BsonDocument>("members");
var memberDocs = members.Select(m =>
{
    var membership = memberships.FirstOrDefault(ms => ms.MemberID == m.MemberID);
    return new BsonDocument
    {
        { "_id", m.MemberID },
        { "name", m.Name },
        { "email", m.Email ?? "" },
        { "trainer", m.Trainer != null
            ? new BsonDocument { { "trainerID", m.TrainerID ?? 0 }, { "name", m.Trainer.Name } }
            : BsonNull.Value },
        { "membership", membership != null
            ? new BsonDocument
            {
                { "membershipID", membership.MembershipID },
                { "subscriptionType", membership.Subscription.Type },
                { "price", (double)membership.Subscription.Price },
                { "startDate", membership.StartDate.ToDateTime(TimeOnly.MinValue) }
            }
            : BsonNull.Value },
        { "bookings", new BsonArray(
            bookings.Where(b => b.MemberID == m.MemberID)
                    .Select(b => new BsonDocument
                    {
                        { "bookingID", b.BookingID },
                        { "classID", b.ClassID },
                        { "className", b.Class.Name }
                    })) }
    };
}).ToList();

await membersCol.InsertManyAsync(memberDocs);
Console.WriteLine($"  MongoDB: {memberDocs.Count} members indsat.");

var classesCol = mongoDB.GetCollection<BsonDocument>("classes");
var classDocs = classes.Select(c => new BsonDocument
{
    { "_id", c.ClassID },
    { "name", c.Name },
    { "classDate", c.ClassDate },
    { "trainer", new BsonDocument { { "trainerID", c.TrainerID }, { "name", c.Trainer.Name } } },
    { "participantCount", bookings.Count(b => b.ClassID == c.ClassID) }
}).ToList();

await classesCol.InsertManyAsync(classDocs);
Console.WriteLine($"  MongoDB: {classDocs.Count} classes indsat.");

var subsCol = mongoDB.GetCollection<BsonDocument>("subscriptions");
var subDocs = subscriptions.Select(s => new BsonDocument
{
    { "_id", s.SubscriptionID },
    { "type", s.Type },
    { "price", (double)s.Price },
    { "memberCount", memberships.Count(ms => ms.SubscriptionID == s.SubscriptionID) }
}).ToList();

await subsCol.InsertManyAsync(subDocs);
Console.WriteLine($"  MongoDB: {subDocs.Count} subscriptions indsat.");

// ── Neo4j ─────────────────────────────────────────────────────────────────────
Console.WriteLine("\nMigrerer til Neo4j...");

var neo4jDriver = GraphDatabase.Driver(neo4jUri, AuthTokens.Basic(neo4jUser, neo4jPass)); // ← Fix: ingen cast
await using var session = neo4jDriver.AsyncSession();

await session.RunAsync("MATCH (n) DETACH DELETE n");

foreach (var t in trainers)
    await session.RunAsync("CREATE (t:Trainer {trainerID: $id, name: $name})",
        new { id = t.TrainerID, name = t.Name });
Console.WriteLine($"  Neo4j: {trainers.Count} Trainer noder oprettet.");

foreach (var s in subscriptions)
    await session.RunAsync("CREATE (s:Subscription {subscriptionID: $id, type: $type, price: $price})",
        new { id = s.SubscriptionID, type = s.Type, price = (double)s.Price });

foreach (var m in members)
{
    await session.RunAsync("CREATE (m:Member {memberID: $id, name: $name, email: $email})",
        new { id = m.MemberID, name = m.Name, email = m.Email ?? "" });

    if (m.TrainerID.HasValue)
        await session.RunAsync(
            "MATCH (m:Member {memberID: $mId}), (t:Trainer {trainerID: $tId}) CREATE (m)-[:TRAINED_BY]->(t)",
            new { mId = m.MemberID, tId = m.TrainerID.Value });
}
Console.WriteLine($"  Neo4j: {members.Count} Member noder oprettet.");

foreach (var c in classes)
{
    await session.RunAsync("CREATE (c:FitnessClass {classID: $id, name: $name, classDate: $date})",
        new { id = c.ClassID, name = c.Name, date = c.ClassDate.ToString("yyyy-MM-dd HH:mm") });
    await session.RunAsync(
        "MATCH (c:FitnessClass {classID: $cId}), (t:Trainer {trainerID: $tId}) CREATE (t)-[:TEACHES]->(c)",
        new { cId = c.ClassID, tId = c.TrainerID });
}
Console.WriteLine($"  Neo4j: {classes.Count} FitnessClass noder oprettet.");

foreach (var b in bookings)
    await session.RunAsync(
        "MATCH (m:Member {memberID: $mId}), (c:FitnessClass {classID: $cId}) CREATE (m)-[:BOOKED]->(c)",
        new { mId = b.MemberID, cId = b.ClassID });
Console.WriteLine($"  Neo4j: {bookings.Count} BOOKED relationer oprettet.");

foreach (var ms in memberships)
    await session.RunAsync(
        "MATCH (m:Member {memberID: $mId}), (s:Subscription {subscriptionID: $sId}) MERGE (m)-[:HAS_SUBSCRIPTION {since: $since}]->(s)",
        new { mId = ms.MemberID, sId = ms.SubscriptionID, since = ms.StartDate.ToString("yyyy-MM-dd") });
Console.WriteLine($"  Neo4j: {memberships.Count} HAS_SUBSCRIPTION relationer oprettet.");

await neo4jDriver.CloseAsync(); // ← tilføj denne
await neo4jDriver.DisposeAsync();
await neo4jDriver.DisposeAsync();

Console.WriteLine("\n✅ Migration fuldført! Data er nu i MySQL, MongoDB og Neo4j.");