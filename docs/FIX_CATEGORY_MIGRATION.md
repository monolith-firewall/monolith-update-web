# Fix: Missing Category Column in Production Database

## Problem
The production database is missing the `Category` column in the `MonolithPackages` table, causing HTTP 500 errors when accessing `/api/v1/packages`.

Error: `SQLite Error 1: 'no such column: m.Category'`

## Quick Fix (Recommended)

### Option 1: Apply Migration via Docker Container

If your production server is running in Docker:

```bash
# 1. Find the running container
docker ps | grep monolith-update

# 2. Execute migration inside the container
docker exec -it <container-name> bash -c "cd /app && dotnet ef database update"

# OR if dotnet-ef is not installed in the container, use SQL directly:
docker exec -it <container-name> sqlite3 /app/data/monolith_updates.db "ALTER TABLE MonolithPackages ADD COLUMN Category TEXT NOT NULL DEFAULT 'Other';"
```

### Option 2: Direct SQL Fix (Fastest)

If you have direct access to the database file:

```bash
# Find the database file (usually in /app/data/ or mounted volume)
sqlite3 /path/to/monolith_updates.db <<EOF
ALTER TABLE MonolithPackages ADD COLUMN Category TEXT NOT NULL DEFAULT 'Other';
EOF
```

### Option 3: Apply via Application Startup

You can modify the application to auto-apply migrations on startup by adding this to `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
```

## Verification

After applying the fix, verify the column exists:

```bash
sqlite3 /path/to/monolith_updates.db "PRAGMA table_info(MonolithPackages);" | grep Category
```

Or test the API:

```bash
curl https://updates.monolithfirewall.com/api/v1/packages?version=1.0.0
```

## Notes

- The migration adds a `Category` column with default value "Other"
- Existing packages will automatically get "Other" as their category
- You can update categories later via the admin UI
