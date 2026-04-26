// FitnessCenterr.Migrator – One-time migration: MySQL → MongoDB + Neo4j

using FitnessCenterr.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Neo4j.Driver;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var mysqlConn   = config.GetConnectionString("DefaultConnection")!;
var mongoConn   = config["MongoDB:ConnectionString"]!;
var mongoDbName = config["MongoDB:Database"]!;
var neo4jUri    = config["Neo4j:Uri"]!;
var neo4jUser   = config["Neo4j:Username"]!;
var neo4jPass   = config["Neo4j:Password"]!;

Console.WriteLine("Henter data fra MySQL...");

var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseMySql(mysqlConn, new MySqlServerVersion(new Version(8, 0, 0)))
    .Options;

using var db = new AppDbContext(dbOptions);

var trainers      = await db.Trainers.ToListAsync();
var members       = await db.Members.Include(m => m.Trainer).ToListAsync();
var subscriptions = await db.Subscriptions.ToListAsync();
var memberships   = await db.Memberships.Include(m => m.Member).Include(m => m.Subscription).ToListAsync();
var classes       = await db.Classes.Include(c => c.Trainer).Include(c => c.Hall).Include(c => c.Location).ToListAsync();
var bookings      = await db.ClassBookings.Include(b => b.Member).Include(b => b.Class).ToListAsync();
var locations     = await db.Locations.ToListAsync();
var centers       = await db.Centers.Include(c => c.Location).ToListAsync();
var halls         = await db.Halls.ToListAsync();
var equipments    = await db.Equipments.ToListAsync();
var vending       = await db.VendingMachines.ToListAsync();
var stocks        = await db.VendingMachineStocks.Include(s => s.VendingMachine).ToListAsync();
var staffs        = await db.Staffs.ToListAsync();
var payments      = await db.Payments.Include(p => p.Member).ToListAsync();

Console.WriteLine($"  Trainers: {trainers.Count}, Members: {members.Count}, Classes: {classes.Count}");
Console.WriteLine($"  Locations: {locations.Count}, Centers: {centers.Count}, Halls: {halls.Count}");
Console.WriteLine($"  Staff: {staffs.Count}, Payments: {payments.Count}");

// ── MongoDB ───────────────────────────────────────────────────────────────────
Console.WriteLine("\nMigrerer til MongoDB...");

var mongoClient = new MongoClient(mongoConn);
var mongoDB     = mongoClient.GetDatabase(mongoDbName);

await mongoDB.DropCollectionAsync("members");
await mongoDB.DropCollectionAsync("classes");
await mongoDB.DropCollectionAsync("subscriptions");
await mongoDB.DropCollectionAsync("centers");
await mongoDB.DropCollectionAsync("staff");
await mongoDB.DropCollectionAsync("payments");

// Members collection
var membersCol = mongoDB.GetCollection<BsonDocument>("members");
var memberDocs = members.Select(m =>
{
    var membership = memberships.FirstOrDefault(ms => ms.MemberID == m.MemberID);
    var memberPayments = payments.Where(p => p.MemberID == m.MemberID).ToList();
    return new BsonDocument
    {
        { "_id", m.MemberID },
        { "name", m.Name },
        { "email", m.Email ?? "" },
        { "birthDate", m.BirthDate.HasValue ? m.BirthDate.Value.ToDateTime(TimeOnly.MinValue) : BsonNull.Value },
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
                    })) },
        { "payments", new BsonArray(
            memberPayments.Select(p => new BsonDocument
            {
                { "paymentID", p.PaymentID },
                { "amount", p.Amount.HasValue ? (double)p.Amount.Value : 0 },
                { "paymentDate", p.PaymentDate ?? DateTime.MinValue },
                { "paymentType", p.PaymentType ?? "" }
            })) }
    };
}).ToList();

await membersCol.InsertManyAsync(memberDocs);
Console.WriteLine($"  MongoDB: {memberDocs.Count} members indsat.");

// Classes collection
var classesCol = mongoDB.GetCollection<BsonDocument>("classes");
var classDocs = classes.Select(c => new BsonDocument
{
    { "_id", c.ClassID },
    { "name", c.Name },
    { "classDate", c.ClassDate },
    { "trainer", new BsonDocument { { "trainerID", c.TrainerID }, { "name", c.Trainer.Name } } },
    { "hall", c.Hall != null ? new BsonDocument { { "hallID", c.HallID ?? 0 }, { "name", c.Hall.Name ?? "" } } : BsonNull.Value },
    { "location", c.Location != null ? new BsonDocument { { "locationID", c.LocationID ?? 0 }, { "city", c.Location.City } } : BsonNull.Value },
    { "participantCount", bookings.Count(b => b.ClassID == c.ClassID) }
}).ToList();

await classesCol.InsertManyAsync(classDocs);
Console.WriteLine($"  MongoDB: {classDocs.Count} classes indsat.");

// Subscriptions collection
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

// Centers collection
var centersCol = mongoDB.GetCollection<BsonDocument>("centers");
var centerDocs = centers.Select(c => new BsonDocument
{
    { "_id", c.CenterID },
    { "location", new BsonDocument { { "locationID", c.LocationID }, { "city", c.Location.City } } },
    { "halls", new BsonArray(halls.Where(h => h.CenterID == c.CenterID).Select(h => new BsonDocument { { "hallID", h.HallID }, { "name", h.Name ?? "" } })) },
    { "equipment", new BsonArray(equipments.Where(e => e.CenterID == c.CenterID).Select(e => new BsonDocument { { "equipmentID", e.EquipmentID }, { "name", e.Name ?? "" } })) },
    { "vendingMachines", new BsonArray(vending.Where(v => v.CenterID == c.CenterID).Select(v => new BsonDocument
    {
        { "vendingMachineID", v.VendingMachineID },
        { "name", v.Name ?? "" },
        { "location", v.Location ?? "" },
        { "stock", new BsonArray(stocks.Where(s => s.VendingMachineID == v.VendingMachineID).Select(s => new BsonDocument
        {
            { "product", s.ProductName ?? "" },
            { "quantity", s.Quantity ?? 0 },
            { "price", s.Price.HasValue ? (double)s.Price.Value : 0 }
        })) }
    })) }
}).ToList();

await centersCol.InsertManyAsync(centerDocs);
Console.WriteLine($"  MongoDB: {centerDocs.Count} centers indsat.");

// Staff collection
var staffCol = mongoDB.GetCollection<BsonDocument>("staff");
var staffDocs = staffs.Select(s => new BsonDocument
{
    { "_id", s.StaffID },
    { "name", s.Name },
    { "role", s.Role ?? "" }
}).ToList();

await staffCol.InsertManyAsync(staffDocs);
Console.WriteLine($"  MongoDB: {staffDocs.Count} staff indsat.");

// ── Neo4j ─────────────────────────────────────────────────────────────────────
Console.WriteLine("\nMigrerer til Neo4j...");

var neo4jDriver = GraphDatabase.Driver(neo4jUri, AuthTokens.Basic(neo4jUser, neo4jPass));
await using var session = neo4jDriver.AsyncSession();

await session.RunAsync("MATCH (n) DETACH DELETE n");

foreach (var t in trainers)
    await session.RunAsync("CREATE (t:Trainer {trainerID: $id, name: $name})",
        new { id = t.TrainerID, name = t.Name });
Console.WriteLine($"  Neo4j: {trainers.Count} Trainer noder oprettet.");

foreach (var s in subscriptions)
    await session.RunAsync("CREATE (s:Subscription {subscriptionID: $id, type: $type, price: $price})",
        new { id = s.SubscriptionID, type = s.Type, price = (double)s.Price });

foreach (var l in locations)
    await session.RunAsync("CREATE (l:Location {locationID: $id, city: $city})",
        new { id = l.LocationID, city = l.City });
Console.WriteLine($"  Neo4j: {locations.Count} Location noder oprettet.");

foreach (var c in centers)
    await session.RunAsync(
        "MATCH (l:Location {locationID: $lId}) CREATE (c:Center {centerID: $id})-[:LOCATED_IN]->(l)",
        new { id = c.CenterID, lId = c.LocationID });
Console.WriteLine($"  Neo4j: {centers.Count} Center noder oprettet.");

foreach (var s in staffs)
    await session.RunAsync("CREATE (s:Staff {staffID: $id, name: $name, role: $role})",
        new { id = s.StaffID, name = s.Name, role = s.Role ?? "" });
Console.WriteLine($"  Neo4j: {staffs.Count} Staff noder oprettet.");

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
    if (c.LocationID.HasValue)
        await session.RunAsync(
            "MATCH (c:FitnessClass {classID: $cId}), (l:Location {locationID: $lId}) CREATE (c)-[:HELD_AT]->(l)",
            new { cId = c.ClassID, lId = c.LocationID.Value });
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

await neo4jDriver.CloseAsync();
await neo4jDriver.DisposeAsync();

Console.WriteLine("\n✅ Migration fuldført! Data er nu i MySQL, MongoDB og Neo4j.");