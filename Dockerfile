# =============================================================================
# Dockerfile for FFmpeg .NET Wrapper
# Author: Vladyslav Zaiets | https://sarmkadan.com
# =============================================================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files
COPY ["FFmpegDotnetWrapper.csproj", "./"]
COPY ["src/", "src/"]

# Restore and build
RUN dotnet restore
RUN dotnet build -c Release --no-restore

# Publish
RUN dotnet publish -c Release -o /app --no-build

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# Install FFmpeg and other dependencies
RUN apt-get update && \
    apt-get install -y \
    ffmpeg \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Verify FFmpeg installation
RUN ffmpeg -version | head -1

WORKDIR /app

# Copy published files from build stage
COPY --from=build /app .

# Create directories for input/output
RUN mkdir -p /data/input && \
    mkdir -p /data/output && \
    mkdir -p /tmp/ffmpeg-work

# Set permissions
RUN chmod -R 755 /data && chmod -R 755 /tmp/ffmpeg-work

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Environment
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Run application
ENTRYPOINT ["dotnet", "FFmpegDotnetWrapper.dll"]
