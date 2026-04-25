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

## Step 5 – Test the API
1. Open http://localhost:5002 in your browser
2. Register an admin user:
```json
POST /api/auth/register
{
  "username": "admin",
  "password": "Admin123!",
  "role": "Admin"
}
```
3. Login and copy the token
4. Click **Authorize** in Swagger and paste: `Bearer {token}`

## Step 6 – Run the Migrator (optional)
Make sure MongoDB and Neo4j are running first, then:

1. Update `FitnessCenter.Migrator/appsettings.json` with your connection strings
2. Run:
```bash
cd FitnessCenter.Migrator
dotnet restore
dotnet run
```

## Step 7 – Start MongoDB
```bash
net start MongoDB
```
Or open MongoDB Compass and connect to: `mongodb://localhost:27017`

## Step 8 – Start Neo4j
1. Open Neo4j Desktop
2. Start your local instance
3. Connect with password: `password`

---

## Project Structure
FitnessCenter/
├── FitnessCenter.API/          ← Web API (controllers, auth, swagger)
├── FitnessCenter.Core/         ← Models and DTOs
├── FitnessCenter.Infrastructure/ ← EF Core, AppDbContext
└── FitnessCenter.Migrator/     ← Migration to MongoDB and Neo4j

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
