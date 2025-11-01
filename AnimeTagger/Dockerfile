FROM python:3.13-slim
LABEL authors="orbital"

WORKDIR /app

# Install grpc-health-probe
# ADD https://github.com/grpc-ecosystem/grpc-health-probe/releases/download/v0.4.14/grpc_health_probe-linux-amd64 /bin/grpc_health_probe
# RUN chmod +x /bin/grpc_health_probe

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

EXPOSE 50051

COPY . .

#HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
#$  CMD grpc_health_probe -addr=:50051 || exit 1

ENTRYPOINT ["python", "main.py"]
