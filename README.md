# Monolith Firewall Update Site

A comprehensive ASP.NET Core web application for managing and distributing Monolith Firewall updates and package modules.

## Features

### Admin Interface
- **Dashboard**: Overview of updates, packages, and recent activity
- **Firewall Updates Management**: Upload and manage firewall firmware updates
- **Packages Management**: Create and manage package types (VPN, IDS, Web Filter, etc.)
- **Package Updates Management**: Upload and manage updates for specific packages
- **Authentication**: Secure admin login with ASP.NET Core Identity

### Update API
- **Firewall Update Check**: Endpoint for firewalls to check for available updates
- **Package Update Check**: Endpoint for checking package-specific updates
- **Package List**: Get list of all available packages
- **File Download**: Secure download endpoints for update files
- **Version Comparison**: Automatic version comparison using Semantic Versioning
- **Hash Verification**: SHA256 file hashes for integrity verification

## Technology Stack

- **Framework**: ASP.NET Core 10.0
- **Database**: SQLite with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Razor Pages with Bootstrap 5
- **API**: RESTful JSON API

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Windows, macOS, or Linux

### Installation

1. **Clone or navigate to the project directory**:
   ```bash
   cd Monolith-Firewall-UpdateSite
   ```

2. **Using helper scripts** (recommended):
   ```bash
   # Build the project
   ./scripts/build.sh

   # Run in development mode with hot reload
   ./scripts/run-dev.sh
   ```

3. **Manual setup**:
   ```bash
   # Navigate to source directory
   cd src

   # Restore NuGet packages
   dotnet restore

   # Run the application (auto-migrates database on startup)
   dotnet run
   ```

4. **Access the application**:
   - Navigate to `http://localhost:5000` (or the URL shown in console)
   - Default admin credentials:
     - Email: `admin@monolith.com`
     - Password: `Admin123!`
   - **⚠️ Change the password immediately after first login!**

## Docker Deployment

### Quick Start with Docker Compose (Recommended)

1. **Create required directories**:
   ```bash
   mkdir -p data wwwroot/updates
   ```

2. **Start the application**:
   ```bash
   docker-compose up -d
   ```

3. **Access the application**:
   - Navigate to `http://localhost:5000`
   - Default admin credentials (same as above)

4. **View logs**:
   ```bash
   docker-compose logs -f
   ```

5. **Stop the application**:
   ```bash
   docker-compose down
   ```

### Manual Docker Build

```bash
# Build the image
docker build -t monolith-updatesite:latest .

# Run the container
docker run -d \
  --name monolith-updatesite \
  -p 5000:5000 \
  -v $(pwd)/data:/app/data \
  -v $(pwd)/wwwroot/updates:/app/wwwroot/updates \
  -e ASPNETCORE_ENVIRONMENT=Production \
  monolith-updatesite:latest
```

### Using GitHub Container Registry

The application is automatically built and published to GitHub Container Registry via GitHub Actions.

```bash
# Pull the latest image
docker pull ghcr.io/<your-username>/monolith-firewall-updatesite:latest

# Run the container
docker run -d \
  --name monolith-updatesite \
  -p 5000:5000 \
  -v $(pwd)/data:/app/data \
  -v $(pwd)/wwwroot/updates:/app/wwwroot/updates \
  ghcr.io/<your-username>/monolith-firewall-updatesite:latest
```

### Docker Volumes

The application uses two persistent volumes:

- **Database Volume** (`./data`): Contains the SQLite database
- **Updates Volume** (`./wwwroot/updates`): Contains uploaded update files

**Important**: Always backup these directories before updates!

### GitHub Actions CI/CD

The repository includes automated Docker builds:

- **Triggers**: Push to `main`/`master`/`develop`, version tags (`v*.*.*`), or pull requests
- **Registry**: GitHub Container Registry (ghcr.io)
- **Security**: Includes Trivy vulnerability scanning
- **Testing**: Automated docker-compose testing

See [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md) for detailed deployment instructions and production recommendations.

## Project Structure

```
Monolith-Firewall-UpdateSite/
├── .github/
│   └── workflows/          # GitHub Actions CI/CD
├── docs/                   # Documentation
│   ├── DEPLOYMENT.md      # Detailed deployment guide
│   └── DOCKER-QUICKSTART.md # Docker quick reference
├── scripts/               # Helper scripts
│   ├── build.sh          # Build the project
│   ├── run-dev.sh        # Run in development mode
│   ├── backup.sh         # Backup database and files
│   └── restore.sh        # Restore from backup
├── src/                   # Application source code
│   ├── Controllers/
│   │   ├── Admin/        # Admin panel controllers
│   │   ├── Api/          # API controllers
│   │   └── AccountController.cs
│   ├── Models/
│   │   ├── Domain/       # Database entities
│   │   ├── ApiResponses/ # API response models
│   │   └── ViewModels/   # View models for forms
│   ├── Services/         # Business logic
│   ├── Data/             # DbContext and configurations
│   ├── Views/            # Razor views
│   ├── wwwroot/          # Static files
│   └── Program.cs        # Application entry point
├── data/                  # Database storage (created at runtime)
├── backups/              # Backup files (created by backup script)
├── docker-compose.yml    # Docker Compose configuration
├── Dockerfile            # Docker image configuration
├── .gitignore
└── README.md
```

## API Documentation

### Base URL
```
https://your-domain.com/api/v1
```

### Endpoints

#### 1. Check Firewall Update

**GET** `/firewall/check-update`

Check if a firewall update is available.

**Query Parameters**:
- `currentVersion` (required): Current firewall version (e.g., "1.0.0")
- `hardwareId` (optional): Hardware ID for tracking

**Example Request**:
```
GET /api/v1/firewall/check-update?currentVersion=1.0.0
```

**Example Response**:
```json
{
  "updateAvailable": true,
  "latestVersion": "1.2.0",
  "currentVersion": "1.0.0",
  "releaseDate": "2026-01-01T00:00:00Z",
  "downloadUrl": "https://your-domain.com/api/v1/firewall/download/1.2.0",
  "fileSize": 52428800,
  "fileHash": "a3d5c...",
  "isCritical": true,
  "releaseNotes": "Critical security update...",
  "minimumCurrentVersion": "0.9.0",
  "message": "Critical update available!"
}
```

#### 2. Download Firewall Update

**GET** `/firewall/download/{version}`

Download a specific firewall update file.

**Example Request**:
```
GET /api/v1/firewall/download/1.2.0
```

**Response**: Binary file stream

---

#### 3. Check Package Update

**GET** `/packages/check-update`

Check if a package update is available.

**Query Parameters**:
- `packageCode` (required): Package identifier (e.g., "MPKG-VPN")
- `currentVersion` (required): Current package version (e.g., "1.0.0")
- `firewallVersion` (optional): Current firewall version for compatibility check

**Example Request**:
```
GET /api/v1/packages/check-update?packageCode=MPKG-VPN&currentVersion=1.0.0&firewallVersion=1.2.0
```

**Example Response**:
```json
{
  "updateAvailable": true,
  "packageCode": "MPKG-VPN",
  "packageName": "VPN Module",
  "latestVersion": "1.1.0",
  "currentVersion": "1.0.0",
  "releaseDate": "2026-01-01T00:00:00Z",
  "downloadUrl": "https://your-domain.com/api/v1/packages/download/MPKG-VPN/1.1.0",
  "fileSize": 10485760,
  "fileHash": "b4e6f...",
  "isCritical": false,
  "releaseNotes": "Bug fixes and improvements...",
  "requiredFirewallVersion": "1.0.0",
  "message": "New update available."
}
```

#### 4. Download Package Update

**GET** `/packages/download/{packageCode}/{version}`

Download a specific package update file.

**Example Request**:
```
GET /api/v1/packages/download/MPKG-VPN/1.1.0
```

**Response**: Binary file stream

---

#### 5. List Available Packages

**GET** `/packages/list`

Get a list of all available packages.

**Query Parameters**:
- `firewallVersion` (optional): Filter packages by firewall version compatibility

**Example Request**:
```
GET /api/v1/packages/list?firewallVersion=1.2.0
```

**Example Response**:
```json
{
  "packages": [
    {
      "packageCode": "MPKG-VPN",
      "packageName": "VPN Module",
      "description": "VPN connectivity module",
      "latestVersion": "1.1.0",
      "releaseDate": "2026-01-01T00:00:00Z"
    },
    {
      "packageCode": "MPKG-IDS",
      "packageName": "IDS Module",
      "description": "Intrusion Detection System",
      "latestVersion": "2.0.0",
      "releaseDate": "2026-01-01T00:00:00Z"
    }
  ]
}
```

## File Structure

```
MonolithUpdateSite/
├── Controllers/
│   ├── Api/                  # API Controllers
│   │   ├── FirewallUpdateController.cs
│   │   └── PackageUpdateController.cs
│   ├── Admin/                # Admin Controllers
│   │   ├── DashboardController.cs
│   │   ├── FirewallUpdatesController.cs
│   │   ├── PackagesController.cs
│   │   └── PackageUpdatesController.cs
│   └── AccountController.cs
├── Models/
│   ├── Domain/               # Database Models
│   ├── ApiResponses/         # API Response Models
│   └── ViewModels/           # View Models
├── Data/
│   └── ApplicationDbContext.cs
├── Services/
│   ├── FileStorageService.cs
│   └── UpdateService.cs
├── Views/                    # Razor Views
├── wwwroot/
│   └── updates/              # Update Files Storage
│       ├── firewall/
│       └── packages/
└── Program.cs
```

## Database Schema

### FirewallUpdates Table
- Version, ReleaseDate, FileName, FileSize, FileHash
- ReleaseNotes, IsActive, IsCritical
- MinimumCurrentVersion, CreatedAt, UpdatedAt

### MonolithPackages Table
- PackageName, PackageCode, Description
- IsActive, CreatedAt

### PackageUpdates Table
- PackageId (FK), Version, ReleaseDate
- FileName, FileSize, FileHash, ReleaseNotes
- IsActive, IsCritical, MinimumPackageVersion
- RequiredFirewallVersion, CreatedAt, UpdatedAt

## Version Comparison

The system uses **Semantic Versioning (SemVer)** for all version comparisons:
- Format: `Major.Minor.Patch` (e.g., "2.1.5")
- Automatic comparison to determine update availability
- Minimum version requirements supported

## Security Features

- **Authentication**: Required for all admin routes
- **File Validation**: File size and type validation on upload
- **Hash Verification**: SHA256 hashes for file integrity
- **HTTPS**: Enforced for all communications
- **Anti-Forgery Tokens**: CSRF protection on all forms

## Customizing API Responses

API responses are defined as C# models in `Models/ApiResponses/`. You can easily modify these models to change the response structure:

1. Edit the model class (e.g., `FirewallUpdateResponse.cs`)
2. Update the corresponding service method (e.g., in `UpdateService.cs`)
3. Rebuild and deploy

Example:
```csharp
// Add a new field to FirewallUpdateResponse
public string ChangelogUrl { get; set; }
```

## Production Deployment

1. **Change Database**: Update connection string in `appsettings.json` to use a production database
2. **Change Admin Password**: Update default admin credentials in `Program.cs`
3. **Enable HTTPS**: Configure SSL certificate
4. **Configure File Storage**: Consider using cloud storage for large files
5. **Add Rate Limiting**: Implement rate limiting for API endpoints
6. **Add Logging**: Configure production logging (Serilog, Application Insights, etc.)

## License

Copyright © 2026 Monolith Firewall Update Site

## Support

For issues and questions, please contact your system administrator.
