# Docker Quick Start Guide

Quick reference for Docker deployment commands.

## First Time Setup

```bash
# Create required directories
mkdir -p data wwwroot/updates

# Start the application
docker-compose up -d

# View logs
docker-compose logs -f
```

Access at: http://localhost:5000

**Default Login:**
- Email: admin@monolith.com
- Password: Admin123!

## Common Commands

### Start/Stop

```bash
# Start services
docker-compose up -d

# Stop services (preserves data)
docker-compose down

# Stop and remove volumes (⚠️ deletes all data!)
docker-compose down -v

# Restart services
docker-compose restart
```

### Logs and Monitoring

```bash
# View all logs
docker-compose logs -f

# View last 100 lines
docker-compose logs --tail=100

# Check container status
docker ps

# Check container health
docker inspect monolith-updatesite | grep -A 10 Health
```

### Updates

```bash
# Pull latest image
docker-compose pull

# Recreate containers with latest image
docker-compose up -d --force-recreate

# Rebuild from source
docker-compose up -d --build
```

### Backup and Restore

```bash
# Backup database and files
tar -czf backup_$(date +%Y%m%d_%H%M%S).tar.gz data/ wwwroot/updates/

# Restore from backup
tar -xzf backup_20260103_120000.tar.gz
docker-compose restart
```

### Troubleshooting

```bash
# Enter container shell
docker exec -it monolith-updatesite /bin/bash

# Check container logs
docker logs monolith-updatesite

# Inspect container
docker inspect monolith-updatesite

# Remove and recreate container
docker-compose down
docker-compose up -d
```

### Clean Up

```bash
# Remove stopped containers
docker container prune

# Remove unused images
docker image prune

# Remove unused volumes
docker volume prune

# Full cleanup (⚠️ be careful!)
docker system prune -a
```

## Manual Docker Commands

### Build

```bash
docker build -t monolith-updatesite:latest .
```

### Run

```bash
docker run -d \
  --name monolith-updatesite \
  -p 5000:5000 \
  -v $(pwd)/data:/app/data \
  -v $(pwd)/wwwroot/updates:/app/wwwroot/updates \
  -e ASPNETCORE_ENVIRONMENT=Production \
  --restart unless-stopped \
  monolith-updatesite:latest
```

### Stop/Remove

```bash
docker stop monolith-updatesite
docker rm monolith-updatesite
```

## GitHub Container Registry

### Login

```bash
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin
```

### Pull and Run

```bash
# Pull latest
docker pull ghcr.io/<username>/monolith-firewall-updatesite:latest

# Run
docker run -d \
  --name monolith-updatesite \
  -p 5000:5000 \
  -v $(pwd)/data:/app/data \
  -v $(pwd)/wwwroot/updates:/app/wwwroot/updates \
  ghcr.io/<username>/monolith-firewall-updatesite:latest
```

## Production Tips

1. **Always backup before updates**
   ```bash
   tar -czf backup_$(date +%Y%m%d).tar.gz data/ wwwroot/updates/
   ```

2. **Use docker-compose for easier management**
   - Defined in `docker-compose.yml`
   - Handles networking, volumes, and environment automatically

3. **Monitor logs regularly**
   ```bash
   docker-compose logs -f --tail=100
   ```

4. **Set up automated backups**
   - Add a cron job for daily backups
   - Store backups on separate storage/server

5. **Use a reverse proxy (nginx/Traefik) for HTTPS**

6. **Change default admin password immediately!**

## Need Help?

- Full documentation: [DEPLOYMENT.md](DEPLOYMENT.md)
- Main README: [README.md](README.md)
- GitHub Issues: Open an issue for support
