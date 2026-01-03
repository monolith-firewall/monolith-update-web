# Deployment Guide - Monolith Firewall Update Site

This guide covers deploying the Monolith Firewall Update Site using Docker.

## Prerequisites

- Docker Engine 20.10+
- Docker Compose 2.0+
- 2GB RAM minimum
- 10GB disk space for updates storage

## Quick Start with Docker Compose

### 1. Clone the Repository

```bash
git clone <your-repo-url>
cd Monolith-Firewall-UpdateSite
```

### 2. Create Required Directories

```bash
mkdir -p data wwwroot/updates
```

### 3. Start the Application

```bash
docker-compose up -d
```

The application will be available at `http://localhost:5000`

### 4. Default Credentials

- **Email:** admin@monolith.com
- **Password:** Admin123!

**⚠️ IMPORTANT:** Change the default password immediately after first login!

## Docker Deployment Options

### Option 1: Using Docker Compose (Recommended)

```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

### Option 2: Manual Docker Build and Run

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

### Option 3: Pull from GitHub Container Registry

```bash
# Login to GitHub Container Registry
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin

# Pull the image
docker pull ghcr.io/<your-username>/monolith-firewall-updatesite:latest

# Run the container
docker run -d \
  --name monolith-updatesite \
  -p 5000:5000 \
  -v $(pwd)/data:/app/data \
  -v $(pwd)/wwwroot/updates:/app/wwwroot/updates \
  ghcr.io/<your-username>/monolith-firewall-updatesite:latest
```

## Volume Management

The application uses two persistent volumes:

### Database Volume
- **Path:** `./data`
- **Contains:** SQLite database (`monolith_updates.db`)
- **Backup:** `cp data/monolith_updates.db data/monolith_updates.db.backup`

### Updates Volume
- **Path:** `./wwwroot/updates`
- **Contains:** Uploaded firewall and package update files
- **Structure:**
  ```
  wwwroot/updates/
  ├── firewall/
  │   └── <version>/
  │       └── <update-files>
  └── packages/
      └── <package-code>/
          └── <version>/
              └── <package-files>
  ```

## Environment Variables

Configure the application using environment variables:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ASPNETCORE_URLS=http://+:5000
  - ConnectionStrings__DefaultConnection=Data Source=/app/data/monolith_updates.db
```

## GitHub Actions CI/CD

The repository includes automated Docker builds via GitHub Actions.

### Automatic Builds

The workflow automatically builds and pushes Docker images when:
- Pushing to `main`, `master`, or `develop` branches
- Creating version tags (e.g., `v1.0.0`)
- Opening pull requests (build only, no push)

### Image Tags

Images are tagged as follows:
- `latest` - Latest build from default branch
- `main`, `develop` - Branch-specific builds
- `v1.0.0`, `v1.0`, `v1` - Semantic version tags
- `main-abc1234` - SHA-based tags

### Manual Trigger

You can manually trigger the workflow from the GitHub Actions tab.

## Production Deployment

### Recommended Configuration

1. **Use a Reverse Proxy (nginx/Traefik)**

```nginx
server {
    listen 80;
    server_name updates.monolith.local;

    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

2. **Enable HTTPS**

Use Let's Encrypt or your own certificates:

```bash
# Using Certbot
certbot --nginx -d updates.monolith.local
```

3. **Set Resource Limits**

Update `docker-compose.yml`:

```yaml
services:
  monolith-updatesite:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '0.5'
          memory: 512M
```

4. **Configure Backup Strategy**

```bash
# Automated backup script
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
tar -czf backup_${DATE}.tar.gz data/ wwwroot/updates/
# Upload to S3/backup server
```

## Monitoring

### Health Check

```bash
curl http://localhost:5000/Account/Login
```

### View Container Logs

```bash
docker logs -f monolith-updatesite
```

### Container Stats

```bash
docker stats monolith-updatesite
```

## Troubleshooting

### Container Won't Start

```bash
# Check logs
docker logs monolith-updatesite

# Check if port is already in use
netstat -tulpn | grep 5000
```

### Database Issues

```bash
# Access container shell
docker exec -it monolith-updatesite /bin/bash

# Check database file
ls -lh /app/data/
```

### Permission Issues

```bash
# Fix volume permissions
sudo chown -R 1000:1000 data/ wwwroot/updates/
```

## Security Recommendations

1. **Change Default Password** - Immediately after deployment
2. **Use HTTPS** - Enable SSL/TLS in production
3. **Restrict Network Access** - Use firewall rules
4. **Regular Updates** - Keep base images updated
5. **Scan for Vulnerabilities** - Use Trivy (included in GitHub Actions)
6. **Backup Regularly** - Automated backups of database and files

## API Endpoints

The application exposes the following API endpoints for firewalls:

- `GET /api/v1/firewall/check-update?currentVersion=1.0.0`
- `GET /api/v1/firewall/download/{version}`
- `GET /api/v1/packages/check-update?packageCode=vpn&currentVersion=1.0.0`
- `GET /api/v1/packages/download/{packageCode}/{version}`
- `GET /api/v1/packages/list?firewallVersion=1.0.0`

## Support

For issues and questions, please open an issue on GitHub.
