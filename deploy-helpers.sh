#!/bin/bash

# CMC Deployment Helper Scripts

# Funktion: Server Status prüfen
check_status() {
    echo "🔍 CMC Application Status Check"
    echo "================================"

    echo "📊 Application Service:"
    sudo systemctl status cmc-app.service --no-pager || echo "❌ Service not found"

    echo ""
    echo "🐳 Database Container:"
    docker ps --filter "name=cmc-postgres-prod" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

    echo ""
    echo "🌐 Nginx Status:"
    sudo systemctl status nginx --no-pager || echo "❌ Nginx not running"

    echo ""
    echo "🔗 Port 80 Check:"
    sudo netstat -tlnp | grep :80 || echo "❌ Nothing listening on port 80"

    echo ""
    echo "📡 Application Health Check:"
    curl -I http://localhost:80 2>/dev/null | head -n 1 || echo "❌ App not responding on port 80"
}

# Funktion: Logs anzeigen
show_logs() {
    echo "📋 CMC Application Logs (last 50 lines)"
    echo "======================================="

    echo "🚀 Application Logs:"
    sudo journalctl -u cmc-app.service -n 50 --no-pager

    echo ""
    echo "🌐 Nginx Error Logs:"
    sudo tail -n 20 /var/log/nginx/cmc-app.error.log 2>/dev/null || echo "No nginx error logs"

    echo ""
    echo "🐳 Database Logs:"
    docker logs cmc-postgres-prod --tail 20 2>/dev/null || echo "Database container not running"
}

# Funktion: Application neu starten
restart_app() {
    echo "🔄 Restarting CMC Application"
    echo "============================="

    echo "🛑 Stopping application..."
    sudo systemctl stop cmc-app.service

    echo "⏳ Waiting 5 seconds..."
    sleep 5

    echo "🚀 Starting application..."
    sudo systemctl start cmc-app.service

    echo "📊 Status check:"
    sudo systemctl status cmc-app.service --no-pager
}

# Funktion: Database Migration
migrate_database() {
    echo "🔄 Running Database Migrations"
    echo "=============================="

    cd /var/www/cmc-app

    echo "🛑 Stopping application for migration..."
    sudo systemctl stop cmc-app.service

    echo "📊 Running migrations..."
    sudo -u cmc-user dotnet CMC.Web.dll --migrate-database

    echo "🚀 Starting application..."
    sudo systemctl start cmc-app.service

    echo "✅ Migration completed"
}

# Funktion: Backup erstellen
create_backup() {
    echo "💾 Creating CMC Backup"
    echo "======================"

    BACKUP_DIR="/var/backups/cmc-app"
    DATE=$(date +%Y%m%d_%H%M%S)

    sudo mkdir -p $BACKUP_DIR

    echo "📁 Backing up application files..."
    sudo tar -czf $BACKUP_DIR/cmc-app-$DATE.tar.gz -C /var/www cmc-app

    echo "🗄️  Backing up database..."
    docker exec cmc-postgres-prod pg_dump -U cmc_user -d cmc_production > $BACKUP_DIR/database-$DATE.sql

    echo "✅ Backup created in $BACKUP_DIR"
    ls -la $BACKUP_DIR/
}

# Funktion: SSL mit Let's Encrypt einrichten
setup_ssl() {
    echo "🔒 Setting up SSL with Let's Encrypt"
    echo "===================================="

    read -p "Enter your domain name: " DOMAIN

    if [[ -z "$DOMAIN" ]]; then
        echo "❌ Domain required"
        return 1
    fi

    echo "📦 Installing Certbot..."
    sudo apt update
    sudo apt install -y certbot python3-certbot-nginx

    echo "🔒 Obtaining SSL certificate..."
    sudo certbot --nginx -d $DOMAIN

    echo "⚙️  Setting up auto-renewal..."
    sudo systemctl enable certbot.timer
    sudo systemctl start certbot.timer

    echo "✅ SSL setup completed!"
}

# Hauptmenü
case "$1" in
    "status")
        check_status
        ;;
    "logs")
        show_logs
        ;;
    "restart")
        restart_app
        ;;
    "migrate")
        migrate_database
        ;;
    "backup")
        create_backup
        ;;
    "ssl")
        setup_ssl
        ;;
    *)
        echo "🛠️  CMC Deployment Helper"
        echo "======================="
        echo ""
        echo "Usage: $0 {status|logs|restart|migrate|backup|ssl}"
        echo ""
        echo "Commands:"
        echo "  status   - Check application and services status"
        echo "  logs     - Show recent application logs"
        echo "  restart  - Restart the CMC application"
        echo "  migrate  - Run database migrations"
        echo "  backup   - Create backup of app and database"
        echo "  ssl      - Setup SSL certificate with Let's Encrypt"
        echo ""
        echo "Examples:"
        echo "  ./deploy-helpers.sh status"
        echo "  ./deploy-helpers.sh logs"
        echo "  ./deploy-helpers.sh restart"
        ;;
esac
