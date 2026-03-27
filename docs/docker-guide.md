# Docker Guide
## Quick Start with Docker
1. Install Docker on your machine
2. Run `docker build -t ffmpeg-dotnet-wrapper .` in the project root
3. Run `docker run -p 8080:8080 ffmpeg-dotnet-wrapper` to start the container
## Docker Compose Usage
1. Install Docker Compose on your machine
2. Run `docker-compose up` in the project root to start the services
## Environment Variables Reference
* `FFMPEG_INPUT_FILE`: input file path
* `FFMPEG_OUTPUT_FILE`: output file path
* `FFMPEG_TRANSCODE_SETTINGS`: transcode settings in JSON format
## Production Deployment Checklist
1. Configure Docker Compose for production
2. Set up a reverse proxy with NGINX or Apache
3. Configure logging and monitoring