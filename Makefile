# ==== Vars ====
WEB_DIR         := ./src/CMC.Web
TW_IN           := $(WEB_DIR)/Styles/app.css
TW_OUT          := $(WEB_DIR)/wwwroot/style.css
TW_PID          := tailwind.pid
TW_LOG          := tailwind.log
TWCLI           := npx @tailwindcss/cli@latest
TW_MODE         ?= bg       # bg | term (gnome-terminal/xterm)

# ==== Docker Dev ====
all: docker-dev
	@echo "✅ CMC started in development mode with Docker"

down: docker-down
	@echo "✅ CMC stopped"

clean: docker-clean
	@echo "✅ Everything cleaned"

docker-dev: docker-setup
	@echo "🚀 Starting CMC with Docker (PostgreSQL + pgAdmin + App)…"
	docker-compose -f docker-compose.yml up -d postgres pgadmin
	@echo "⏳ Waiting for services to be ready…"
	@sleep 10
	@$(MAKE) dev-migrate
	@$(MAKE) tailwind-start
	@echo ""
	@echo "✅ Services ready!"
	@echo "   📡 Application: http://localhost:5000 | https://localhost:5001"
	@echo "   📊 pgAdmin: http://localhost:8080 (admin@example.com / admin)"
	@echo "   🗄️  PostgreSQL: localhost:5432 (postgres / password)"
	@echo "   🧪 Test Login: test@example.com / password123"
	@echo ""
	cd src/CMC.Web && dotnet run

docker-setup:
	@echo "📁 Setting up Docker configuration files…"
	@mkdir -p docker
	@if [ ! -f docker/postgres-init.sql ]; then \
		echo "Creating postgres-init.sql…"; \
		echo "-- Automatic database initialization" > docker/postgres-init.sql; \
		echo "SELECT 'CREATE DATABASE cmc_dev' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'cmc_dev')\\gexec" >> docker/postgres-init.sql; \
	fi
	@echo "✅ Docker setup complete"

docker-down:
	@echo "🛑 Stopping all Docker services…"
	docker-compose -f docker-compose.yml down

docker-clean:
	@echo "🧹 Cleaning all Docker resources…"
	docker-compose -f docker-compose.yml down --volumes --remove-orphans
	docker system prune -f

# ==== DB & Tests ====
dev-migrate:
	@echo "🔄 Running database migrations…"
	cd src/CMC.Web && dotnet ef database update --project ../CMC.Infrastructure
	@echo "✅ Database migrations completed"

dev-test:
	@echo "🧪 Running tests…"
	dotnet test

# ==== Help ====
help:
	@echo "🎯 CMC Development Commands:"
	@echo ""
	@echo "Main:"
	@echo "  make docker-dev              # Start all (DB + pgAdmin + App + Tailwind)"
	@echo "  make down / make clean       # Stop / Prune"
	@echo ""
	@echo "Tailwind:"
	@echo "  make tailwind-start          # Start watcher (TW_MODE=bg|term)"
	@echo "  make tailwind-stop           # Stop watcher"
	@echo "  make tailwind-status         # Show PID & last logs"
	@echo "  make tailwind-build          # Build prod"
	@echo ""
	@echo "DB/Tests:"
	@echo "  make dev-migrate             # EF migrations"
	@echo "  make dev-test                # dotnet test"


# ==== Tailwind (plattformneutral) ====
tailwind-ensure:
	@mkdir -p $(WEB_DIR)/Styles $(WEB_DIR)/wwwroot
	@if [ ! -f "$(TW_IN)" ]; then \
		echo "➕ Creating $(TW_IN)"; \
		printf '@import "tailwindcss";\n@source "../";\n' > $(TW_IN); \
	fi

tailwind-bg: tailwind-ensure
	@echo "🎨 Starting Tailwind v4 watcher in background..."
	@rm -f $(TW_PID) $(TW_LOG)
	@(cd $(WEB_DIR) && $(TWCLI) -i ./Styles/app.css -o ./wwwroot/style.css -w) > $(TW_LOG) 2>&1 & echo $$! > $(TW_PID)
	@echo "✅ Tailwind watcher started (PID: $$(cat $(TW_PID)))"

tailwind-term: tailwind-ensure
	@if command -v gnome-terminal >/dev/null; then \
		echo "🪟 gnome-terminal detected — starting Tailwind…"; \
		gnome-terminal -- bash -c "cd '$(WEB_DIR)'; $(TWCLI) -i ./Styles/app.css -o ./wwwroot/style.css -w; exec bash"; \
	elif command -v xterm >/dev/null; then \
		echo "🪟 xterm detected — starting Tailwind…"; \
		xterm -e "cd '$(WEB_DIR)' && $(TWCLI) -i ./Styles/app.css -o ./wwwroot/style.css -w"; \
	else \
		echo "⚠️  No GUI terminal found — falling back to background mode"; \
		$(MAKE) tailwind-bg; \
	fi

tailwind-start:
	@if [ "$(TW_MODE)" = "term" ]; then \
		$(MAKE) tailwind-term; \
	else \
		$(MAKE) tailwind-bg; \
	fi

tailwind-stop:
	@echo "🛑 Stopping Tailwind watcher..."
	@if [ -f $(TW_PID) ]; then \
		kill $$(cat $(TW_PID)) 2>/dev/null || true; \
		rm -f $(TW_PID); \
		echo "✅ Tailwind watcher stopped"; \
	else \
		echo "ℹ️  No Tailwind PID file found"; \
	fi

tailwind-status:
	@echo "ℹ️  Tailwind PID: $$(test -f $(TW_PID) && cat $(TW_PID) || echo '-')"
	@echo "ℹ️  CSS exists: $$(test -f $(TW_OUT) && echo YES || echo NO)"
	@echo "📋 Last logs:"; tail -n 15 $(TW_LOG) 2>/dev/null || echo "no logs"

tailwind-build: tailwind-ensure
	@echo "🏗️  Building Tailwind (prod, minify)…"
	@$(TWCLI) -i $(WEB_DIR)/Styles/app.css -o $(WEB_DIR)/wwwroot/style.css --minify --cwd $(WEB_DIR)
	@echo "✅ Built $$(wc -c < $(TW_OUT)) bytes"


.PHONY: all down clean docker-dev docker-setup docker-down docker-clean \
	tailwind-ensure tailwind-bg tailwind-term tailwind-start tailwind-stop \
	tailwind-status tailwind-build dev-migrate dev-test help
