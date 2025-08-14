# CMC Development Makefile - Vollständige Docker-Integration

# Main Commands
all: docker-dev
	@echo "✅ CMC started in development mode with Docker"

down: docker-down
	@echo "✅ CMC stopped"

clean: docker-clean
	@echo "✅ Everything cleaned"

# Docker Development (Empfohlen)
docker-dev: docker-setup
	@echo "🚀 Starting CMC with Docker (PostgreSQL + pgAdmin + App)..."
	docker-compose up -d postgres pgadmin
	@echo "⏳ Waiting for services to be ready..."
	@sleep 10
	@$(MAKE) dev-migrate
	@echo ""
	@echo "✅ Services ready!"
	@echo "   📡 Application: http://localhost:5000 | https://localhost:5001"
	@echo "   📊 pgAdmin: http://localhost:8080 (admin@example.com / admin)"
	@echo "   🗄️  PostgreSQL: localhost:5432 (postgres / password)"
	@echo "   🧪 Test Login: test@example.com / password123"
	@echo ""
	@echo "🔧 pgAdmin ist bereits konfiguriert - Server 'CMC Development' sollte automatisch verfügbar sein!"
	cd src/CMC.Web && dotnet run

docker-setup:
	@echo "📁 Setting up Docker configuration files..."
	@mkdir -p docker
	@if [ ! -f docker/postgres-init.sql ]; then \
		echo "Creating postgres-init.sql..."; \
		echo "-- Automatic database initialization" > docker/postgres-init.sql; \
		echo "SELECT 'CREATE DATABASE cmc_dev' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'cmc_dev')\\gexec" >> docker/postgres-init.sql; \
	fi
	@echo "✅ Docker setup complete"

docker-down:
	@echo "🛑 Stopping all Docker services..."
	docker-compose down

docker-clean:
	@echo "🧹 Cleaning all Docker resources..."
	docker-compose down --volumes --remove-orphans
	docker system prune -f

# Legacy Commands (für Rückwärtskompatibilität)
dev-db:
	@echo "⚠️  Legacy command - use 'make docker-dev' instead"
	@$(MAKE) docker-dev

dev-run: docker-dev

dev-migrate:
	@echo "🔄 Running database migrations..."
	cd src/CMC.Web && dotnet ef database update --project ../CMC.Infrastructure
	@echo "✅ Database migrations completed"

dev-test:
	@echo "🧪 Running tests..."
	dotnet test

# Debugging
debug-docker:
	@echo "🔍 Docker Debug Information:"
	@echo ""
	@echo "📋 Running Containers:"
	@docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
	@echo ""
	@echo "🌐 Networks:"
	@docker network ls | grep cmc || echo "No CMC networks found"
	@echo ""
	@echo "💾 Volumes:"
	@docker volume ls | grep cmc || echo "No CMC volumes found"
	@echo ""
	@echo "🗄️  Database Test:"
	@docker exec postgres-cmc-compose psql -U postgres -c "\\l" 2>/dev/null || echo "❌ Database not accessible"

logs:
	@echo "📋 Service Logs:"
	docker-compose logs --tail=50

logs-postgres:
	docker logs postgres-cmc-compose --tail=50

logs-pgadmin:
	docker logs pgadmin-cmc --tail=50

# Help
help:
	@echo "🎯 CMC Development Commands:"
	@echo ""
	@echo "Main Commands:"
	@echo "  make docker-dev    # Start everything (PostgreSQL + pgAdmin + App)"
	@echo "  make down          # Stop all services"
	@echo "  make clean         # Stop and remove everything"
	@echo ""
	@echo "Utilities:"
	@echo "  make dev-migrate   # Run database migrations"
	@echo "  make dev-test      # Run tests"
	@echo "  make debug-docker  # Show Docker debug info"
	@echo "  make logs          # Show all service logs"
	@echo ""
	@echo "🌐 Services:"
	@echo "  App:      http://localhost:5000"
	@echo "  App:      https://localhost:5001"
	@echo "  pgAdmin:  http://localhost:8080"
	@echo ""
	@echo "🔑 Credentials:"
	@echo "  App:      test@example.com / password123"
	@echo "  pgAdmin:  admin@example.com / admin"
	@echo "  Postgres: postgres / password"

.PHONY: all down clean docker-dev docker-setup docker-down docker-clean dev-db dev-run dev-migrate dev-test debug-docker logs logs-postgres logs-pgadmin help
