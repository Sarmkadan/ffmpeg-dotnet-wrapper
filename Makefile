# =============================================================================
# Makefile for FFmpeg .NET Wrapper
# Author: Vladyslav Zaiets | https://sarmkadan.com
# Common development tasks and build commands
# =============================================================================

.PHONY: help build test clean restore publish docker docker-compose docker-clean

# Default target
.DEFAULT_GOAL := help

DOTNET := dotnet
DOCKER := docker
DOCKER_IMAGE := ffmpeg-wrapper:latest
DOCKER_REGISTRY := ghcr.io/vladyslav-zaiets/ffmpeg-wrapper

# Color output
BLUE := \033[0;34m
GREEN := \033[0;32m
YELLOW := \033[0;33m
RED := \033[0;31m
NC := \033[0m # No Color

help: ## Display this help message
	@echo "$(BLUE)FFmpeg .NET Wrapper - Build Tasks$(NC)"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "$(GREEN)%-20s$(NC) %s\n", $$1, $$2}'
	@echo ""
	@echo "$(YELLOW)Examples:$(NC)"
	@echo "  make build         # Build the project"
	@echo "  make test          # Run tests"
	@echo "  make docker-build  # Build Docker image"
	@echo "  make pack          # Create NuGet package"

restore: ## Restore NuGet dependencies
	@echo "$(BLUE)Restoring dependencies...$(NC)"
	$(DOTNET) restore
	@echo "$(GREEN)✓ Restore complete$(NC)"

clean: ## Clean build outputs
	@echo "$(BLUE)Cleaning...$(NC)"
	$(DOTNET) clean
	rm -rf bin obj nupkg
	@echo "$(GREEN)✓ Clean complete$(NC)"

build: restore ## Build the project
	@echo "$(BLUE)Building project...$(NC)"
	$(DOTNET) build -c Release
	@echo "$(GREEN)✓ Build complete$(NC)"

rebuild: clean build ## Clean and rebuild

test: build ## Run unit tests
	@echo "$(BLUE)Running tests...$(NC)"
	$(DOTNET) test -c Release --no-build
	@echo "$(GREEN)✓ Tests passed$(NC)"

format: ## Format code style (check only)
	@echo "$(BLUE)Checking code format...$(NC)"
	$(DOTNET) format --verify-no-changes --verbosity diagnostic
	@echo "$(GREEN)✓ Code format OK$(NC)"

format-fix: ## Format and fix code style
	@echo "$(BLUE)Formatting code...$(NC)"
	$(DOTNET) format
	@echo "$(GREEN)✓ Code formatted$(NC)"

lint: ## Lint code (StyleCop)
	@echo "$(BLUE)Linting code...$(NC)"
	$(DOTNET) build -c Release /p:TreatWarningsAsErrors=true
	@echo "$(GREEN)✓ Lint passed$(NC)"

pack: build ## Create NuGet package
	@echo "$(BLUE)Creating NuGet package...$(NC)"
	$(DOTNET) pack -c Release -o ./nupkg
	@echo "$(GREEN)✓ Package created in ./nupkg$(NC)"

publish-local: pack ## Publish to local NuGet (requires nuget source configured)
	@echo "$(BLUE)Publishing to local NuGet...$(NC)"
	$(DOTNET) nuget push ./nupkg/*.nupkg --source local
	@echo "$(GREEN)✓ Package published$(NC)"

run: build ## Run the application
	@echo "$(BLUE)Running application...$(NC)"
	$(DOTNET) run -c Release --project src/Program.cs

docker-build: ## Build Docker image
	@echo "$(BLUE)Building Docker image: $(DOCKER_IMAGE)$(NC)"
	$(DOCKER) build -t $(DOCKER_IMAGE) .
	@echo "$(GREEN)✓ Docker image built$(NC)"

docker-run: docker-build ## Run Docker container
	@echo "$(BLUE)Running Docker container...$(NC)"
	$(DOCKER) run -p 5000:5000 \
		-v $$(pwd)/data/input:/data/input \
		-v $$(pwd)/data/output:/data/output \
		--name ffmpeg-wrapper \
		$(DOCKER_IMAGE)

docker-stop: ## Stop Docker container
	@echo "$(BLUE)Stopping Docker container...$(NC)"
	$(DOCKER) stop ffmpeg-wrapper 2>/dev/null || true
	$(DOCKER) rm ffmpeg-wrapper 2>/dev/null || true
	@echo "$(GREEN)✓ Container stopped$(NC)"

docker-clean: docker-stop ## Remove Docker image
	@echo "$(BLUE)Removing Docker image...$(NC)"
	$(DOCKER) rmi $(DOCKER_IMAGE) 2>/dev/null || true
	@echo "$(GREEN)✓ Docker image removed$(NC)"

docker-compose-up: ## Start docker-compose stack
	@echo "$(BLUE)Starting docker-compose...$(NC)"
	mkdir -p data/input data/output data/temp
	$(DOCKER) compose up -d
	@echo "$(GREEN)✓ Services started$(NC)"
	@echo "   API: http://localhost:5000"

docker-compose-down: ## Stop docker-compose stack
	@echo "$(BLUE)Stopping docker-compose...$(NC)"
	$(DOCKER) compose down
	@echo "$(GREEN)✓ Services stopped$(NC)"

docker-compose-logs: ## View docker-compose logs
	$(DOCKER) compose logs -f

info: ## Display project information
	@echo "$(BLUE)FFmpeg .NET Wrapper$(NC)"
	@echo "Author:         Vladyslav Zaiets"
	@echo "Website:        https://sarmkadan.com"
	@echo "Repository:     https://github.com/vladyslav-zaiets/ffmpeg-dotnet-wrapper"
	@echo ""
	@echo "$(YELLOW)Build Info:$(NC)"
	@$(DOTNET) --version
	@echo ""
	@echo "$(YELLOW)FFmpeg:$(NC)"
	@ffmpeg -version 2>/dev/null | head -1 || echo "FFmpeg not installed"

setup: ## Initial project setup
	@echo "$(BLUE)Setting up project...$(NC)"
	$(DOTNET) restore
	mkdir -p data/input data/output data/temp
	mkdir -p .github/workflows
	@echo "$(GREEN)✓ Setup complete$(NC)"

docs: ## Generate documentation
	@echo "$(BLUE)Generating documentation...$(NC)"
	@if [ -d "docs" ]; then \
		echo "Documentation files:"; \
		ls -la docs/; \
	else \
		echo "$(RED)docs/ directory not found$(NC)"; \
	fi

verify-ffmpeg: ## Verify FFmpeg installation
	@echo "$(BLUE)Verifying FFmpeg...$(NC)"
	@which ffmpeg > /dev/null && \
		echo "$(GREEN)✓ FFmpeg found at: $$(which ffmpeg)$(NC)" || \
		echo "$(RED)✗ FFmpeg not found in PATH$(NC)"
	@ffmpeg -version 2>/dev/null | head -3 || true

examples: ## List available examples
	@echo "$(BLUE)Available Examples:$(NC)"
	@ls -1 examples/*.cs 2>/dev/null | while read file; do \
		echo "  - $$(basename $$file)"; \
	done

all: clean restore build test pack ## Full build pipeline
	@echo "$(GREEN)✓ All tasks complete!$(NC)"

ci: verify-ffmpeg build test pack ## CI pipeline (local)
	@echo "$(GREEN)✓ CI pipeline complete!$(NC)"

.PHONY: setup info examples verify-ffmpeg docs
