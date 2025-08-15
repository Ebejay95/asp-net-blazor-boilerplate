all: docker-dev
	@echo "✅ CMC started in development mode with Docker"

down: docker-down tailwind-stop
	@echo "✅ CMC stopped"

clean: docker-clean tailwind-stop
	@echo "✅ Everything cleaned"

# Docker Development (Empfohlen)
docker-dev: docker-setup tailwind-start
	@echo "🚀 Starting CMC with Docker (PostgreSQL + pgAdmin + App)..."
	docker-compose -f docker-compose.yml up -d postgres pgadmin
	@echo "⏳ Waiting for services to be ready..."
	@sleep 10
	@$(MAKE) dev-migrate
	@echo ""
	@echo "✅ Services ready!"
	@echo "   📡 Application: http://localhost:5000 | https://localhost:5001"
	@echo "   📊 pgAdmin: http://localhost:8080 (admin@example.com / admin)"
	@echo "   🗄️  PostgreSQL: localhost:5432 (postgres / password)"
	@echo "   🧪 Test Login: test@example.com / password123"
	@echo "   🎨 Tailwind: watching for changes..."
	@echo ""
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

# Tailwind CSS v4 Commands
tailwind-setup:
	@echo "🚀 Setting up Tailwind CSS v4..."
	@npm install -D tailwindcss@latest
	@npx tailwindcss init
	@echo "✅ Tailwind setup complete"

tailwind-start:
	@echo "🎨 Starting Tailwind CSS watcher (latest version)..."
	@if [ -f tailwind.pid ]; then \
		echo "⚠️  Tailwind bereits running (PID: $$(cat tailwind.pid))"; \
	else \
		mkdir -p ./src/CMC.Web/Styles ./src/CMC.Web/wwwroot; \
		if [ ! -f ./src/CMC.Web/Styles/app.css ]; then \
			echo "Creating input CSS..."; \
			echo "@tailwind base;" > ./src/CMC.Web/Styles/app.css; \
			echo "@tailwind components;" >> ./src/CMC.Web/Styles/app.css; \
			echo "@tailwind utilities;" >> ./src/CMC.Web/Styles/app.css; \
		fi; \
		nohup npx tailwindcss \
			-i ./src/CMC.Web/Styles/app.css \
			-o ./src/CMC.Web/wwwroot/style.css \
			--watch > tailwind.log 2>&1 & echo $$! > tailwind.pid; \
		echo "✅ Tailwind watcher started (PID: $$(cat tailwind.pid))"; \
		echo "📋 Monitor with: tail -f tailwind.log"; \
	fi

tailwind-stop:
	@echo "🛑 Stopping Tailwind CSS watcher..."
	@if [ -f tailwind.pid ]; then \
		kill $$(cat tailwind.pid) 2>/dev/null || true; \
		rm tailwind.pid; \
		echo "✅ Tailwind watcher stopped"; \
	else \
		echo "ℹ️  Tailwind watcher not running"; \
	fi
	@pkill -f "tailwindcss.*watch" 2>/dev/null || true

tailwind-build:
	@echo "🎨 Building Tailwind CSS (production)..."
	@mkdir -p ./src/CMC.Web/wwwroot
	npx tailwindcss \
		-i ./src/CMC.Web/Styles/app.css \
		-o ./src/CMC.Web/wwwroot/style.css \
		--minify
	@echo "✅ Tailwind CSS built"

tailwind-debug:
	@echo "🔍 Tailwind Debug Information:"
	@echo "Working Directory: $$(pwd)"
	@echo "Node Version: $$(node --version 2>/dev/null || echo 'NOT INSTALLED')"
	@echo "NPM Version: $$(npm --version 2>/dev/null || echo 'NOT INSTALLED')"
	@echo "Tailwind Version: $$(npx tailwindcss --help | head -n 1 2>/dev/null || echo 'NOT INSTALLED')"
	@echo ""
	@echo "File Check:"
	@echo "  Input file: $$(test -f ./src/CMC.Web/Styles/app.css && echo '✅ EXISTS' || echo '❌ MISSING')"
	@echo "  Output file: $$(test -f ./src/CMC.Web/wwwroot/style.css && echo '✅ EXISTS' || echo '❌ MISSING')"
	@echo "  Config file: $$(test -f ./tailwind.config.js && echo '✅ EXISTS' || echo '❌ MISSING')"
	@echo "  Index.razor: $$(test -f ./src/CMC.Web/Pages/Index.razor && echo '✅ EXISTS' || echo '❌ MISSING')"
	@echo ""
	@if [ -f ./src/CMC.Web/wwwroot/style.css ]; then \
		echo "Output CSS Info:"; \
		echo "  File size: $$(wc -c < ./src/CMC.Web/wwwroot/style.css) bytes"; \
		echo "  Contains .bg-red-500: $$(grep -q '\.bg-red-500' ./src/CMC.Web/wwwroot/style.css && echo '✅ YES' || echo '❌ NO')"; \
		echo "  Contains .text-white: $$(grep -q '\.text-white' ./src/CMC.Web/wwwroot/style.css && echo '✅ YES' || echo '❌ NO')"; \
	fi
	@echo ""
	@if [ -f tailwind.log ]; then \
		echo "Recent Tailwind logs:"; \
		tail -n 10 tailwind.log; \
	else \
		echo "❌ No Tailwind logs found"; \
	fi

tailwind-test:
	@echo "🧪 Testing Tailwind CSS..."
	@mkdir -p ./src/CMC.Web/Styles ./src/CMC.Web/wwwroot
	@echo "Creating test HTML with Tailwind classes..."
	@echo '<div class="bg-red-500 text-white p-4">Test</div>' > test.html
	npx tailwindcss -i ./src/CMC.Web/Styles/app.css -o ./test-output.css --content "./test.html"
	@if [ -f test-output.css ]; then \
		echo "✅ Test build successful"; \
		echo "File size: $$(wc -c < test-output.css) bytes"; \
		echo "Contains .bg-red-500: $$(grep -q '\.bg-red-500' test-output.css && echo '✅ YES' || echo '❌ NO')"; \
		rm test.html test-output.css; \
	else \
		echo "❌ Test build failed"; \
	fi

docker-down:
	@echo "🛑 Stopping all Docker services..."
	docker-compose -f docker-compose.yml down

docker-clean:
	@echo "🧹 Cleaning all Docker resources..."
	docker-compose -f docker-compose.yml down --volumes --remove-orphans
	docker system prune -f

# Database Commands
dev-migrate:
	@echo "🔄 Running database migrations..."
	cd src/CMC.Web && dotnet ef database update --project ../CMC.Infrastructure
	@echo "✅ Database migrations completed"

dev-test:
	@echo "🧪 Running tests..."
	dotnet test

# Help
help:
	@echo "🎯 CMC Development Commands:"
	@echo ""
	@echo "Main Commands:"
	@echo "  make docker-dev       # Start everything (PostgreSQL + pgAdmin + App + Tailwind)"
	@echo "  make down             # Stop all services"
	@echo "  make clean            # Stop and remove everything"
	@echo ""
	@echo "Tailwind Commands:"
	@echo "  make tailwind-setup   # Setup Tailwind CSS v4"
	@echo "  make tailwind-start   # Start Tailwind watcher"
	@echo "  make tailwind-stop    # Stop Tailwind watcher"
	@echo "  make tailwind-build   # Build Tailwind (production)"
	@echo "  make tailwind-debug   # Debug Tailwind setup"
	@echo "  make tailwind-test    # Test Tailwind build"
	@echo ""
	@echo "Database:"
	@echo "  make dev-migrate      # Run database migrations"
	@echo "  make dev-test         # Run tests"
	@echo ""
	@echo "🌐 Services:"
	@echo "  App:      http://localhost:5000"
	@echo "  pgAdmin:  http://localhost:8080"
	@echo ""
	@echo "🔑 Credentials:"
	@echo "  App:      test@example.com / password123"
	@echo "  pgAdmin:  admin@example.com / admin"

.PHONY: all down clean docker-dev docker-setup docker-down docker-clean tailwind-setup tailwind-start tailwind-stop tailwind-build tailwind-debug tailwind-test dev-migrate dev-test help
