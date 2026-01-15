# Docker Deployment Guide

This guide explains how to deploy the frontend using Docker and Docker Compose.

## Quick Start

1. **Set the backend URL** in `docker-compose.yml` or via environment variable:
   ```bash
   export NEXT_PUBLIC_API_URL=http://your-backend-url:8080
   ```

2. **Build and run**:
   ```bash
   docker-compose up -d --build
   ```

3. **Access the application**:
   - Frontend: http://localhost:3000

## Configuration

### Backend URL

The backend URL can be configured in several ways:

1. **Environment variable** (recommended):
   ```bash
   export NEXT_PUBLIC_API_URL=http://backend:8080
   docker-compose up -d --build
   ```

2. **docker-compose.yml**:
   ```yaml
   services:
     frontend:
       environment:
         - NEXT_PUBLIC_API_URL=http://backend:8080
   ```

3. **.env file** (create one in the project root):
   ```env
   NEXT_PUBLIC_API_URL=http://backend:8080
   ```

### Important Note

`NEXT_PUBLIC_API_URL` is a build-time variable in Next.js. This means:
- The value is embedded into the JavaScript bundle during the build
- To change it, you need to rebuild the Docker image
- The docker-compose file sets it as both a build argument and runtime environment variable

If you need to change the backend URL:
```bash
docker-compose build --build-arg NEXT_PUBLIC_API_URL=http://new-backend-url:8080
docker-compose up -d
```

## Docker Commands

### Build only:
```bash
docker-compose build
```

### Run in foreground:
```bash
docker-compose up
```

### Run in background:
```bash
docker-compose up -d
```

### View logs:
```bash
docker-compose logs -f frontend
```

### Stop:
```bash
docker-compose down
```

### Rebuild after changes:
```bash
docker-compose up -d --build
```

## Production Considerations

1. **Network**: The docker-compose file creates a network `imagemanager-network`. Make sure your backend is on the same network or accessible from the frontend container.

2. **Port**: The frontend runs on port 3000 by default. You can change this in `docker-compose.yml`:
   ```yaml
   ports:
     - "8080:3000"  # Host:Container
   ```

3. **Environment Variables**: For production, consider using a `.env` file or Docker secrets for sensitive configuration.

4. **Reverse Proxy**: In production, you'll likely want to use a reverse proxy (nginx, traefik, etc.) in front of the Next.js application.

## Troubleshooting

### Container won't start
- Check logs: `docker-compose logs frontend`
- Verify the backend URL is correct and accessible
- Ensure port 3000 is not already in use

### API calls failing
- Verify `NEXT_PUBLIC_API_URL` is set correctly
- Check network connectivity between containers
- Rebuild the image if you changed the backend URL

### Build fails
- Ensure all dependencies are in `package.json`
- Check that `next.config.ts` has `output: 'standalone'`
- Verify Node.js version compatibility (20.x)
