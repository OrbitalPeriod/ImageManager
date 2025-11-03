# syntax=docker/dockerfile:1.8
FROM python:3.13-slim AS base
LABEL authors="orbital"

WORKDIR /app

# Enable BuildKit cache mounts for pip
# and install grpc-health-probe efficiently
ADD https://github.com/grpc-ecosystem/grpc-health-probe/releases/download/v0.4.14/grpc_health_probe-linux-amd64 \
    /bin/grpc_health_probe
RUN chmod +x /bin/grpc_health_probe

# Copy requirements first for better caching
COPY requirements.txt .

# Use a BuildKit cache for pip
RUN --mount=type=cache,target=/root/.cache/pip \
    pip install -r requirements.txt

EXPOSE 50051

# Copy everything else (after dependencies are cached)
COPY . .

# Health check using the gRPC probe
HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
  CMD /bin/grpc_health_probe -addr=:50051 || exit 1

ENTRYPOINT ["python", "main.py"]
