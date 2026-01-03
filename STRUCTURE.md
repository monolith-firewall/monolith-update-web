# Project Structure

This document describes the organization of the Monolith Firewall Update Site project.

## Directory Layout

```
Monolith-Firewall-UpdateSite/
│
├── .github/
│   └── workflows/
│       └── docker-build-deploy.yml    # GitHub Actions CI/CD workflow
│
├── docs/                               # Documentation
│   ├── DEPLOYMENT.md                  # Detailed deployment guide
│   └── DOCKER-QUICKSTART.md           # Quick Docker reference
│
├── scripts/                            # Helper scripts
│   ├── build.sh                       # Build the project
│   ├── run-dev.sh                     # Run in development with hot reload
│   ├── backup.sh                      # Backup database and files
│   └── restore.sh                     # Restore from backup
│
├── src/                                # Application source code
│   ├── Controllers/
│   │   ├── Admin/                     # Admin panel controllers
│   │   │   ├── DashboardController.cs
│   │   │   ├── FirewallVersionsController.cs
│   │   │   ├── FirewallUpdatesController.cs
│   │   │   ├── PackagesController.cs
│   │   │   └── PackageUpdatesController.cs
│   │   ├── Api/                       # API controllers
│   │   │   ├── FirewallUpdateController.cs
│   │   │   └── PackageUpdateController.cs
│   │   └── AccountController.cs       # Authentication
│   │
│   ├── Models/
│   │   ├── Domain/                    # Database entities
│   │   │   ├── FirewallVersion.cs
│   │   │   ├── FirewallUpdate.cs
│   │   │   ├── MonolithPackage.cs
│   │   │   └── PackageUpdate.cs
│   │   ├── ApiResponses/              # API response models
│   │   │   ├── FirewallUpdateResponse.cs
│   │   │   ├── PackageUpdateResponse.cs
│   │   │   └── AvailablePackagesResponse.cs
│   │   └── ViewModels/                # View models for forms
│   │       ├── FirewallVersionViewModel.cs
│   │       ├── FirewallUpdateViewModel.cs
│   │       ├── PackageViewModel.cs
│   │       └── PackageUpdateViewModel.cs
│   │
│   ├── Services/                      # Business logic
│   │   ├── IUpdateService.cs
│   │   ├── UpdateService.cs
│   │   ├── IFileStorageService.cs
│   │   └── FileStorageService.cs
│   │
│   ├── Data/                          # Database context
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations/               # EF Core migrations
│   │
│   ├── Views/                         # Razor views
│   │   ├── Shared/
│   │   │   ├── _Layout.cshtml
│   │   │   └── _ValidationScriptsPartial.cshtml
│   │   ├── Account/
│   │   ├── Dashboard/
│   │   ├── FirewallVersions/
│   │   ├── FirewallUpdates/
│   │   ├── Packages/
│   │   └── PackageUpdates/
│   │
│   ├── wwwroot/                       # Static files
│   │   ├── css/
│   │   ├── js/
│   │   └── updates/                  # Uploaded update files (gitignored)
│   │       ├── firewall/
│   │       └── packages/
│   │
│   ├── Program.cs                     # Application entry point
│   ├── appsettings.json              # Configuration
│   ├── appsettings.Development.json  # Development configuration
│   └── MonolithUpdateSite.csproj     # Project file
│
├── data/                               # SQLite database (created at runtime, gitignored)
├── backups/                           # Backup files (created by scripts, gitignored)
│
├── .dockerignore                      # Docker ignore patterns
├── .gitignore                         # Git ignore patterns
├── docker-compose.yml                 # Docker Compose configuration
├── Dockerfile                         # Docker image definition
├── CONTRIBUTING.md                    # Contribution guidelines
├── README.md                          # Main documentation
└── STRUCTURE.md                       # This file
```

## Key Directories

### `/src/`
Contains all application source code. This is where development happens.

### `/docs/`
Project documentation including deployment guides and references.

### `/scripts/`
Helper scripts for common development and maintenance tasks.

### `/.github/`
GitHub-specific files including CI/CD workflows.

### `/data/` (runtime)
Created automatically when the application runs. Contains the SQLite database.
**Gitignored** - Not committed to repository.

### `/backups/` (runtime)
Created by backup scripts. Contains compressed backups of data and update files.
**Gitignored** - Not committed to repository.

## File Purposes

### Root Configuration Files

- **docker-compose.yml** - Defines services, volumes, and networking for Docker deployment
- **Dockerfile** - Multi-stage build configuration for containerization
- **.gitignore** - Prevents committing build artifacts, databases, and sensitive files
- **.dockerignore** - Excludes unnecessary files from Docker builds

### Source Configuration

- **Program.cs** - Application startup, middleware configuration, dependency injection
- **appsettings.json** - Production configuration (database, logging, etc.)
- **appsettings.Development.json** - Development-specific overrides
- **MonolithUpdateSite.csproj** - NuGet packages and build configuration

## Development Workflow

1. **Code changes** happen in `/src/`
2. **Build** using `./scripts/build.sh` or `dotnet build` in `/src/`
3. **Run** using `./scripts/run-dev.sh` or `dotnet run` in `/src/`
4. **Test** using Docker: `docker-compose up --build`
5. **Deploy** via GitHub Actions or manual Docker commands

## Data Flow

1. **User uploads** update file → Stored in `/src/wwwroot/updates/`
2. **Metadata saved** → SQLite database in `/data/`
3. **Firewall queries** API → Returns update info from database
4. **Firewall downloads** → File served from `/src/wwwroot/updates/`

## Backup Strategy

- **Database**: `/data/monolith_updates.db`
- **Update files**: `/src/wwwroot/updates/`
- **Backup script**: `./scripts/backup.sh` creates compressed archives in `/backups/`
- **Retention**: Last 10 backups kept automatically

## Docker Volumes

When running in Docker, these directories are mounted as volumes:

- `./data:/app/data` - Database persistence
- `./wwwroot/updates:/app/wwwroot/updates` - Update files persistence

This ensures data survives container restarts and updates.
