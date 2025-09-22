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

clean: dump docker-clean
	@echo "✅ Everything cleaned"

docker-dev: docker-setup
	@echo "🚀 Starting CMC with Docker (PostgreSQL + pgAdmin + App)…"
	docker-compose -f docker-compose.yml up -d postgres pgadmin
	@echo "⏳ Waiting for services to be ready…"
	@sleep 10
	@$(MAKE) dev-migrate
	@$(MAKE) tailwind
	@echo ""
	@echo "✅ Services ready!"
	@echo "   📡 Application: http://localhost:5000 | https://localhost:5001"
	@echo "   📊 pgAdmin: http://localhost:8080 (admin@example.com / admin)"
	@echo "   🗄️  PostgreSQL: localhost:5432 (postgres / password)"
	@echo ""
	@$(MAKE) app-watch

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
	cd src/CMC.Web && \
	ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=cmc_dev;Username=postgres;Password=password;Pooling=true;SslMode=Disable;" \
	dotnet ef database update --project ../CMC.Infrastructure
	@echo "✅ Database migrations completed"

dev-test:
	@echo "🧪 Running tests…"
	dotnet test

# ==== App (.NET Hot Reload) ====
app-watch:
	@echo "👀 .NET Hot Reload…"
	cd src/CMC.Web && \
	ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=cmc_dev;Username=postgres;Password=password;Pooling=true;SslMode=Disable;" \
	GRAPH_TENANT_ID="dev-tenant" \
	APP_PUBLIC_BASE_URL="http://localhost:5000" \
	GRAPH_CLIENT_ID="dev-client" \
	GRAPH_CLIENT_ID="dev-client" \
	GRAPH_CLIENT_SECRET="dev-secret" \
	GRAPH_FROM_USER="dev@example.com" \
	DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1 DOTNET_USE_POLLING_FILE_WATCHER=1 \
	dotnet watch --non-interactive run

# ==== Help ====
help:
	@echo "🎯 CMC Development Commands:"
	@echo ""
	@echo "Main:"
	@echo "  make docker-dev              # Start all (DB + pgAdmin + App + Tailwind + Hot Reload)"
	@echo "  make down / make clean       # Stop / Prune"
	@echo ""
	@echo "Tailwind:"
	@echo "  make tailwind-start          # Prebuild + Watch (TW_MODE=bg|term, --poll)"
	@echo "  make tailwind-rebuild        # Force one-shot rebuild"
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
		printf '@import "tailwindcss";\n@source "../**/*.razor";\n@source "../**/*.cshtml";\n@source "../**/*.html";\n@source "../**/*.js";\n@source "../**/*.ts";\n@source "./buttons.css";\n@import "./buttons.css";\n' > $(TW_IN); \
	fi
	@# Stelle sicher, dass buttons.css existiert
	@if [ ! -f "$(WEB_DIR)/Styles/buttons.css" ]; then \
		printf '@layer utilities { a{ @apply bg-red-800 hover:bg-blue-600 text-white px-4 py-2 rounded; } }\n' > $(WEB_DIR)/Styles/buttons.css; \
	fi

# --- One-shot Prebuild (fix für "leere style.css" beim ersten Lauf) ---
tailwind-prebuild: tailwind-ensure
	@echo "🏗️  Tailwind prebuild (dev)…"
	@cd $(WEB_DIR) && $(TWCLI) -i ./Styles/app.css -o ./wwwroot/style.css --verbose
	@# simple sanity check
	@test -s $(TW_OUT) || (echo "❌ style.css ist leer – check @source/@import Pfade" && exit 1)
	@echo "✅ Prebuild ok: $$(wc -c < $(TW_OUT)) bytes"

# --- Watcher ---
tailwind-bg: tailwind-prebuild
	@echo "🎨 Starting Tailwind v4 watcher in background (poll, verbose)…"
	@rm -f $(TW_PID) $(TW_LOG)
	@(cd $(WEB_DIR) && $(TWCLI) -i ./Styles/app.css -o ./wwwroot/style.css -w --poll --verbose) > $(TW_LOG) 2>&1 & echo $$! > $(TW_PID)
	@echo "✅ Tailwind watcher started (PID: $$(cat $(TW_PID)))"

tailwind-term: tailwind-prebuild
	@if command -v gnome-terminal >/dev/null; then \
		echo "🪟 gnome-terminal detected — starting Tailwind…"; \
		gnome-terminal -- bash -c "cd '$(WEB_DIR)'; $(TWCLI) -i ./Styles/app.css -o ./wwwroot/style.css -w --poll --verbose; exec bash"; \
	elif command -v xterm >/dev/null; then \
		echo "🪟 xterm detected — starting Tailwind…"; \
		xterm -e "cd '$(WEB_DIR)' && $(TWCLI) -i ./Styles/app.css -o ./wwwroot/style.css -w --poll --verbose"; \
	else \
		echo "⚠️  No GUI terminal found — falling back to background mode"; \
		$(MAKE) tailwind-bg; \
	fi

tailwind:
	@if [ "$(TW_MODE)" = "term" ]; then \
		$(MAKE) tailwind-term; \
	else \
		$(MAKE) tailwind-bg; \
	fi

tailwind-rebuild: tailwind-ensure
	@echo "🔁 Forcing Tailwind rebuild (dev)…"
	@cd $(WEB_DIR) && $(TWCLI) -i ./Styles/app.css -o ./wwwroot/style.css --verbose
	@test -s $(TW_OUT) || (echo "❌ style.css ist leer – check @source/@import" && exit 1)
	@echo "✅ Rebuilt: $$(wc -c < $(TW_OUT)) bytes"

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
	@echo "📋 Last logs:"; tail -n 30 $(TW_LOG) 2>/dev/null || echo "no logs"

tailwind-build: tailwind-ensure
	@echo "🏗️  Building Tailwind (prod, minify)…"
	@cd $(WEB_DIR) && $(TWCLI) -i ./Styles/app.css -o ./wwwroot/style.css --minify
	@test -s $(TW_OUT) || (echo "❌ style.css ist leer – prod build fehlgeschlagen" && exit 1)
	@echo "✅ Built $$(wc -c < $(TW_OUT)) bytes"

dump:
	docker exec -t postgres-cmc-compose pg_dumpall --globals-only -U postgres > globals.sql

dump-restore:
	docker exec -i postgres-cmc-compose pg_restore -U postgres -d cmc_dev --clean --if-exists < cmc_dev.dump

.PHONY: all down clean docker-dev docker-setup docker-down docker-clean \
	tailwind-ensure tailwind-prebuild tailwind-bg tailwind-term tailwind \
	tailwind-rebuild tailwind-stop tailwind-status tailwind-build \
	dev-migrate dev-test app-watch help
