# Fitness Center API – Installation Guide

## Prerequisites
- .NET 8 SDK (https://dotnet.microsoft.com/download)
- JetBrains Rider or Visual Studio 2022
- MySQL Workbench (https://dev.mysql.com/downloads/workbench/)
- MongoDB Compass (https://www.mongodb.com/try/download/compass)
- Neo4j Desktop (https://neo4j.com/download)

---

## Step 1 – Clone the project
```bash
git clone https://github.com/ali0509-arch/FitnessCenter-API.git
cd FitnessCenter-API
```

## Step 2 – Import the database
1. Open MySQL Workbench
2. Connect to your MySQL server
3. Run `FitnessDBSend.sql` to create tables and insert data
4. Run `users_and_privileges.sql` to create app user

## Step 3 – Configure connection string
Open `FitnessCenter.API/appsettings.json` and update:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=HOST;Port=3306;Database=DB;User=USER;Password=PASSWORD;"
  },
  "Jwt": {
    "Key": "fitness-center-super-secret-key-2024!!",
    "Issuer": "FitnessAPI",
    "Audience": "FitnessAPIUsers",
    "ExpiryMinutes": "60"
  }
}
```

## Step 4 – Run the API
```bash
cd FitnessCenter.API
dotnet restore
dotnet run
```
Swagger UI opens at: **http://localhost:5002**

---

## Step 5 – Authentication (JWT)

This API uses **JWT (JSON Web Token)** for authentication.

### How JWT works:
1. User logs in with username and password
2. Server validates credentials and returns a **JWT token**
3. Client includes the token in every request as a **Bearer token**
4. Server validates the token and grants access

### Token structure:
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9   ← Header
.eyJuYW1lIjoiYWRtaW4iLCJyb2xlIjoiQWRtaW4ifQ  ← Payload (username, role, expiry)
.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c  ← Signature

### Roles:
| Role | Access |
|---|---|
| **Admin** | Full access – CREATE, UPDATE, DELETE |
| **User** | Read-only – GET endpoints only |

---

## Step 6 – Test the API in Swagger

### Register a user:
```json
POST /api/auth/register
{
  "username": "admin",
  "password": "Admin123!",
  "role": "Admin"
}
```

### Login and get token:
```json
POST /api/auth/login
{
  "username": "admin",
  "password": "Admin123!"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "admin",
  "role": "Admin"
}
```

### Authorize in Swagger:
1. Copy the token from the response
2. Click the **Authorize** button 🔒 at the top of Swagger UI
3. Type in the field (WITHOUT "Bearer" – Swagger adds it automatically):
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
4. Click **Authorize** → **Close**
5. Now all protected endpoints are accessible

### Token expiry:
- Token is valid for **12 hours**
- After expiry you need to login again to get a new token

---

## Step 7 – Run the Migrator (optional)
Make sure MongoDB and Neo4j are running first, then:

1. Update `FitnessCenter.Migrator/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=HOST;Port=3306;Database=DB;User=USER;Password=PASSWORD;"
  },
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "fitnessdb"
  },
  "Neo4j": {
    "Uri": "bolt://localhost:7687",
    "Username": "neo4j",
    "Password": "password"
  }
}
```

2. Run:
```bash
cd FitnessCenter.Migrator
dotnet restore
dotnet run
```

Expected output:
Henter data fra MySQL...
Trainers: 21, Members: 11, Classes: 11
Migrerer til MongoDB...
MongoDB: 11 members indsat.
MongoDB: 11 classes indsat.
MongoDB: 3 subscriptions indsat.
Migrerer til Neo4j...
Neo4j: 21 Trainer noder oprettet.
Neo4j: 11 Member noder oprettet.
✅ Migration fuldført!

## Step 8 – Start MongoDB
```bash
net start MongoDB
```
Or open MongoDB Compass and connect to: `mongodb://localhost:27017`

## Step 9 – Start Neo4j
1. Open Neo4j Desktop
2. Start your local instance
3. Connect with password: `password`
4. Run this query to verify data:
```cypher
MATCH (n)-[r]->(m) RETURN n,r,m LIMIT 25
```

---

## Project Structure
FitnessCenter/
├── FitnessCenter.API/            ← Web API (controllers, auth, swagger)
│   ├── Controllers/              ← Auth, Members, Trainers, Classes etc.
│   ├── Program.cs                ← App setup, JWT, Swagger
│   └── appsettings.json          ← Database and JWT config
├── FitnessCenter.Core/           ← Models and DTOs
│   ├── Model/                    ← Member, Trainer, Class etc.
│   └── DTO/                      ← Request/Response objects
├── FitnessCenter.Infrastructure/ ← EF Core, AppDbContext
│   └── Data/AppDbContext.cs      ← Database mapping
└── FitnessCenter.Migrator/       ← Migration to MongoDB and Neo4j
├── Program.cs                ← Migration logic
└── appsettings.json          ← Connection strings

---

## Test Users
| Username | Password | Role |
|---|---|---|
| superadmin | Admin123! | Admin |
| admin | Admin123! | User |

---

## Database Backup
Run the backup script from the project root:
```bash
backup.bat
```
Backup is saved to: `C:\Backups\FitnessDB\`

---

## Security
| Measure | Implementation |
|---|---|
| SQL Injection | EF Core parameterized queries |
| Password hashing | BCrypt (cost factor 10) |
| Authentication | JWT Bearer tokens |
| Authorization | Role-based (Admin/User) |
| Database backup | mysqldump script |
| User privileges | Dedicated app user with minimal rights |
