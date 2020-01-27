# Deployment Guide

Comprehensive guide for deploying FFmpeg .NET Wrapper in production environments.

## Deployment Architectures

### Architecture 1: Standalone CLI

Single machine running transcoding operations via CLI.

**Setup**:
```bash
dotnet publish -c Release -o /opt/ffmpeg-wrapper
chmod +x /opt/ffmpeg-wrapper/FFmpegDotnetWrapper
```

**Usage**:
```bash
/opt/ffmpeg-wrapper/FFmpegDotnetWrapper transcode \
  --input video.mp4 \
  --output video.webm \
  --codec vp9
```

**Pros**: Simple, low overhead
**Cons**: No job queue, synchronous only

---

### Architecture 2: REST API Service

Expose operations via HTTP API.

**Setup**:
```bash
dotnet publish -c Release -o /opt/ffmpeg-api
cd /opt/ffmpeg-api
./FFmpegDotnetWrapper  # Starts on http://localhost:5000
```

**Usage**:
```bash
curl -X POST http://localhost:5000/api/ffmpeg/transcode \
  -H "Content-Type: application/json" \
  -d '{
    "inputPath": "input.mp4",
    "outputPath": "output.webm",
    "videoCodec": "VP9",
    "audioCodec": "Opus"
  }'
```

**systemd Unit** (`/etc/systemd/system/ffmpeg-api.service`):
```ini
[Unit]
Description=FFmpeg .NET Wrapper API
After=network.target

[Service]
Type=simple
User=ffmpeg
WorkingDirectory=/opt/ffmpeg-api
ExecStart=/opt/ffmpeg-api/FFmpegDotnetWrapper
Restart=on-failure
RestartSec=10

[Install]
WantedBy=multi-user.target
```

**Enable**:
```bash
sudo systemctl daemon-reload
sudo systemctl enable ffmpeg-api
sudo systemctl start ffmpeg-api
sudo systemctl status ffmpeg-api
```

**Pros**: Remote access, simple HTTP interface
**Cons**: Single instance, no auto-scaling

---

### Architecture 3: Docker Container

Containerized deployment with isolation.

**Dockerfile**:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "FFmpegDotnetWrapper.dll"]
```

**Build**:
```bash
docker build -t ffmpeg-wrapper:latest .
```

**Run**:
```bash
docker run -d \
  --name ffmpeg-wrapper \
  -p 5000:5000 \
  -v /data/input:/data/input \
  -v /data/output:/data/output \
  ffmpeg-wrapper:latest
```

**Pros**: Isolation, reproducibility, scalable
**Cons**: Containerization overhead

---

### Architecture 4: Kubernetes Deployment

Production-grade multi-instance deployment.

**deployment.yaml**:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: ffmpeg-wrapper
spec:
  replicas: 3
  selector:
    matchLabels:
      app: ffmpeg-wrapper
  template:
    metadata:
      labels:
        app: ffmpeg-wrapper
    spec:
      containers:
      - name: api
        image: ffmpeg-wrapper:latest
        ports:
        - containerPort: 5000
        env:
        - name: ASPNETCORE_URLS
          value: "http://+:5000"
        - name: FFmpegOptions__MaxConcurrentOperations
          value: "2"
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "1Gi"
            cpu: "1000m"
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 5
```

**Service**:
```yaml
apiVersion: v1
kind: Service
metadata:
  name: ffmpeg-wrapper
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 5000
  selector:
    app: ffmpeg-wrapper
```

**Deploy**:
```bash
kubectl apply -f deployment.yaml
kubectl apply -f service.yaml
```

**Pros**: Multi-instance, auto-scaling, load balancing
**Cons**: Complexity, infrastructure requirements

---

### Architecture 5: Background Job Queue

Asynchronous processing with job queue.

**Setup**:
```csharp
// Startup
services.AddFFmpegWrapper(options =>
{
    options.EnableBackgroundJobs = true;
});

services.AddScoped<IJobRepository, InMemoryJobRepository>();
services.AddHostedService<JobProcessorService>();
```

**Usage**:
```csharp
var jobService = serviceProvider.GetRequiredService<BackgroundJobService>();

var jobId = await jobService.EnqueueTranscodeAsync(
    inputFile: "video.mp4",
    outputPath: "/output/",
    settings: new TranscodeSettings { VideoCodec = VideoCodec.VP9 });

// Check status
var status = await jobService.GetJobStatusAsync(jobId);
Console.WriteLine($"Status: {status.State}");  // Pending, Processing, Completed, Failed
```

**Job States**:
- `Pending` – Queued, awaiting processing
- `Processing` – Currently being processed
- `Completed` – Finished successfully
- `Failed` – Error occurred

**Webhook Notification**:
```csharp
// On completion, POST to webhook URL
POST /webhook/ffmpeg
{
  "jobId": "job-123",
  "state": "Completed",
  "result": { "success": true, "outputPath": "..." }
}
```

---

## Production Checklist

### Pre-Deployment

- [ ] FFmpeg installed and verified on target system
- [ ] .NET 10 runtime installed
- [ ] Sufficient disk space for temp files (at least 2x largest input)
- [ ] Network connectivity verified (if API mode)
- [ ] File permissions configured (read input, write output)
- [ ] Logging configured (min LogLevel: Information)
- [ ] Error handling tested

### Security

- [ ] API endpoints behind authentication (basic auth, API key, or OAuth)
- [ ] HTTPS configured (TLS certificates)
- [ ] Input files validated (format, size, content)
- [ ] Output directory restricted (no world-writable)
- [ ] FFmpeg path verified (no symlink attacks)
- [ ] Environment variables not exposed
- [ ] No verbose logging in production (Info level only)

### Performance

- [ ] `MaxConcurrentOperations` tuned to CPU count
- [ ] Working directory on SSD if available
- [ ] Memory limits enforced
- [ ] Timeout values appropriate for expected file sizes
- [ ] Codec selection optimized (H.264 for speed, VP9 for quality)

### Monitoring

- [ ] Structured logging configured (JSON format)
- [ ] Metrics exported (completed ops, success rate, duration)
- [ ] Alerts configured (failures, timeouts, disk space)
- [ ] Health checks implemented (`/health`, `/ready` endpoints)
- [ ] Log aggregation (ELK, Splunk, etc.)

### Backup & Recovery

- [ ] Output files regularly backed up
- [ ] Job metadata persisted (database or Redis)
- [ ] Temporary files cleaned up on failure
- [ ] Recovery procedures tested
- [ ] Disaster recovery plan documented

---

## Configuration for Production

### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "FFmpegDotnetWrapper": "Information"
    },
    "Console": {
      "IncludeScopes": true,
      "TimestampFormat": "yyyy-MM-ddTHH:mm:ss.fffZ"
    }
  },
  "FFmpegOptions": {
    "DefaultTimeout": "00:15:00",
    "EnableDetailedLogging": false,
    "MaxConcurrentOperations": 4,
    "FFmpegPath": "/usr/bin/ffmpeg",
    "WorkingDirectory": "/var/tmp/ffmpeg-work",
    "EnableWebhooks": true,
    "EnableBackgroundJobs": true
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://+:5000"
      }
    }
  }
}
```

### Environment Variables

```bash
# Override appsettings
export FFmpegOptions__DefaultTimeout="00:20:00"
export FFmpegOptions__MaxConcurrentOperations=8
export FFmpegOptions__FFmpegPath="/usr/local/bin/ffmpeg"
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://+:5000"
```

---

## Health Checks

### Endpoint: GET /health

Returns `200 OK` if service is healthy.

```csharp
app.MapGet("/health", async (IFFmpegService ffmpeg) =>
{
    var available = await ffmpeg.IsFFmpegAvailableAsync();
    return available ? Results.Ok() : Results.ServiceUnavailable();
});
```

**Usage**:
```bash
curl http://localhost:5000/health
# 200 OK if healthy
```

### Endpoint: GET /ready

Returns `200 OK` if ready to accept requests.

```csharp
app.MapGet("/ready", async (IFFmpegService ffmpeg, IHostApplicationLifetime lifetime) =>
{
    if (lifetime.ApplicationStarted.IsCancellationRequested)
        return Results.ServiceUnavailable();
    
    var available = await ffmpeg.IsFFmpegAvailableAsync();
    return available ? Results.Ok() : Results.ServiceUnavailable();
});
```

---

## Scaling Strategies

### Horizontal Scaling

**Load Balancer (NGINX)**:
```nginx
upstream ffmpeg_backend {
    server api1.example.com:5000;
    server api2.example.com:5000;
    server api3.example.com:5000;
}

server {
    listen 80;
    server_name api.example.com;
    
    location / {
        proxy_pass http://ffmpeg_backend;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### Vertical Scaling

Increase machine resources:
```csharp
services.AddFFmpegWrapper(options =>
{
    options.MaxConcurrentOperations = 16;  // More CPU cores
});
```

### Queue-Based Scaling

Process jobs asynchronously:
```csharp
// Job enqueue (fast)
var jobId = await jobService.EnqueueTranscodeAsync(input, output, settings);

// Workers process in background (scalable)
var workers = Enumerable.Range(0, 8)
    .Select(_ => ProcessJobsAsync());
```

---

## Monitoring & Observability

### Logging Configuration

```csharp
builder.Logging.AddConsole()
    .AddFile(options =>
    {
        options.RollingInterval = RollingInterval.Day;
        options.LogFilePath = "/var/log/ffmpeg-wrapper/logs-.txt";
    })
    .SetMinimumLevel(LogLevel.Information);
```

### Metrics Export

```csharp
var counter = new PerformanceCounter("FFmpeg Wrapper", "Operations Completed", false);
var timer = Stopwatch.StartNew();

try
{
    await ffmpeg.TranscodeAsync(...);
    counter.Increment();
}
finally
{
    logger.LogInformation("Transcode took {Duration}ms", timer.ElapsedMilliseconds);
}
```

### Sample Prometheus Metrics

```
# HELP ffmpeg_operations_total Total number of operations
# TYPE ffmpeg_operations_total counter
ffmpeg_operations_total{status="success"} 1234
ffmpeg_operations_total{status="failure"} 12

# HELP ffmpeg_operation_duration_seconds Operation duration in seconds
# TYPE ffmpeg_operation_duration_seconds histogram
ffmpeg_operation_duration_seconds_bucket{le="10"} 1000
ffmpeg_operation_duration_seconds_bucket{le="60"} 1200
ffmpeg_operation_duration_seconds_bucket{le="600"} 1245
```

---

## Disaster Recovery

### Data Loss Prevention

1. **Input Files**: Keep original files immutable
2. **Output Files**: Daily backups to separate storage
3. **Job Records**: Database replication or Redis persistence
4. **Logs**: Centralized aggregation (ELK, Splunk)

### Failover Procedures

1. Monitor health checks
2. On failure, remove from load balancer
3. Spawn replacement instance
4. Drain queue to other instances
5. Investigate root cause

### Tested Recovery Procedures

- [ ] Restore from backup
- [ ] Replay failed jobs
- [ ] Scale up during peak
- [ ] Handle disk space exhaustion
- [ ] Restart FFmpeg process
