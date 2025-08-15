#!/bin/bash

# CMC Monitoring & Health Check Setup

echo "📊 Setting up CMC Monitoring & Health Checks"
echo "============================================="

# 1. Health Check Endpoint für die App
echo "🔧 Creating health check endpoint..."
cat > /tmp/health-check.sh << 'EOF'
#!/bin/bash

# CMC Health Check Script
LOG_FILE="/var/log/cmc-app/health-check.log"
APP_URL="http://localhost:5000"
DB_CHECK="http://localhost:5000/api/health/database"

# Function to log with timestamp
log_message() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" >> "$LOG_FILE"
}

# Check application health
check_app_health() {
    if curl -f -s "$APP_URL" > /dev/null 2>&1; then
        log_message "✅ App is healthy"
        return 0
    else
        log_message "❌ App health check failed"
        return 1
    fi
}

# Check database connectivity
check_database() {
    if docker exec cmc-postgres-prod pg_isready -U cmc_user -d cmc_production > /dev/null 2>&1; then
        log_message "✅ Database is healthy"
        return 0
    else
        log_message "❌ Database health check failed"
        return 1
    fi
}

# Check disk space
check_disk_space() {
    DISK_USAGE=$(df /var/www/cmc-app | awk 'NR==2 {print $5}' | sed 's/%//')
    if [ "$DISK_USAGE" -lt 85 ]; then
        log_message "✅ Disk space OK ($DISK_USAGE% used)"
        return 0
    else
        log_message "⚠️  Disk space warning ($DISK_USAGE% used)"
        return 1
    fi
}

# Check memory usage
check_memory() {
    MEMORY_USAGE=$(free | grep '^Mem:' | awk '{printf "%.1f", $3/$2 * 100.0}')
    if (( $(echo "$MEMORY_USAGE < 85" | bc -l) )); then
        log_message "✅ Memory usage OK ($MEMORY_USAGE% used)"
        return 0
    else
        log_message "⚠️  Memory usage warning ($MEMORY_USAGE% used)"
        return 1
    fi
}

# Restart app if needed
restart_app_if_needed() {
    if ! check_app_health; then
        log_message "🔄 Attempting to restart application..."
        sudo systemctl restart cmc-app.service
        sleep 30
        if check_app_health; then
            log_message "✅ Application restarted successfully"
        else
            log_message "❌ Application restart failed"
        fi
    fi
}

# Main health check
main() {
    log_message "🔍 Starting health check..."

    check_app_health
    APP_HEALTHY=$?

    check_database
    DB_HEALTHY=$?

    check_disk_space
    DISK_OK=$?

    check_memory
    MEMORY_OK=$?

    # Restart if app is unhealthy
    if [ $APP_HEALTHY -ne 0 ]; then
        restart_app_if_needed
    fi

    # Overall status
    if [ $APP_HEALTHY -eq 0 ] && [ $DB_HEALTHY -eq 0 ]; then
        log_message "✅ Overall system health: GOOD"
        exit 0
    else
        log_message "❌ Overall system health: ISSUES DETECTED"
        exit 1
    fi
}

# Create log directory if it doesn't exist
sudo mkdir -p /var/log/cmc-app
sudo chown www-data:www-data /var/log/cmc-app

main "$@"
EOF

sudo mv /tmp/health-check.sh /usr/local/bin/cmc-health-check
sudo chmod +x /usr/local/bin/cmc-health-check

# 2. Cron job für regelmäßige Health Checks
echo "⏰ Setting up health check cron job..."
sudo tee /etc/cron.d/cmc-health-check > /dev/null << 'EOF'
# CMC Health Check - every 5 minutes
*/5 * * * * root /usr/local/bin/cmc-health-check >> /var/log/cmc-app/cron.log 2>&1
EOF

# 3. Log rotation für CMC Logs
echo "📋 Setting up log rotation..."
sudo tee /etc/logrotate.d/cmc-app > /dev/null << 'EOF'
/var/log/cmc-app/*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 644 www-data www-data
    postrotate
        systemctl reload cmc-app.service > /dev/null 2>&1 || true
    endscript
}
EOF

# 4. System resource monitoring
echo "📊 Setting up system monitoring..."
sudo tee /usr/local/bin/cmc-system-monitor > /dev/null << 'EOF'
#!/bin/bash

# CMC System Monitor
METRICS_FILE="/var/log/cmc-app/metrics.log"

get_metrics() {
    TIMESTAMP=$(date '+%Y-%m-%d %H:%M:%S')
    CPU_USAGE=$(top -bn1 | grep "Cpu(s)" | awk '{print $2}' | sed 's/%us,//')
    MEMORY_USAGE=$(free | grep '^Mem:' | awk '{printf "%.1f", $3/$2 * 100.0}')
    DISK_USAGE=$(df /var/www/cmc-app | awk 'NR==2 {print $5}' | sed 's/%//')

    # Application specific metrics
    APP_PROCESS=$(pgrep -f "dotnet.*CMC.Web.dll" | wc -l)
    DB_CONNECTIONS=$(docker exec cmc-postgres-prod psql -U cmc_user -d cmc_production -t -c "SELECT count(*) FROM pg_stat_activity;" 2>/dev/null | xargs || echo "0")

    echo "$TIMESTAMP,CPU:${CPU_USAGE}%,MEM:${MEMORY_USAGE}%,DISK:${DISK_USAGE}%,APP_PROC:${APP_PROCESS},DB_CONN:${DB_CONNECTIONS}" >> "$METRICS_FILE"
}

get_metrics
EOF

sudo chmod +x /usr/local/bin/cmc-system-monitor

# 5. Metrics collection cron job
sudo tee -a /etc/cron.d/cmc-health-check > /dev/null << 'EOF'

# CMC System Metrics - every minute
* * * * * root /usr/local/bin/cmc-system-monitor
EOF

# 6. Disk cleanup script
echo "🧹 Setting up automatic cleanup..."
sudo tee /usr/local/bin/cmc-cleanup > /dev/null << 'EOF'
#!/bin/bash

# CMC Cleanup Script
echo "$(date '+%Y-%m-%d %H:%M:%S') - Starting cleanup..." >> /var/log/cmc-app/cleanup.log

# Clean old backups (keep last 7 days)
find /var/backups/cmc-app/ -name "*.tar.gz" -mtime +7 -delete 2>/dev/null || true
find /var/backups/cmc-app/ -name "*.sql" -mtime +7 -delete 2>/dev/null || true

# Clean old Docker images
docker image prune -f > /dev/null 2>&1 || true

# Clean temporary files
find /tmp -name "*cmc*" -mtime +1 -delete 2>/dev/null || true

# Clean old log files (keep last 30 days)
find /var/log/cmc-app/ -name "*.log" -mtime +30 -delete 2>/dev/null || true

echo "$(date '+%Y-%m-%d %H:%M:%S') - Cleanup completed" >> /var/log/cmc-app/cleanup.log
EOF

sudo chmod +x /usr/local/bin/cmc-cleanup

# 7. Weekly cleanup cron job
sudo tee -a /etc/cron.d/cmc-health-check > /dev/null << 'EOF'

# CMC Cleanup - weekly on Sunday at 2 AM
0 2 * * 0 root /usr/local/bin/cmc-cleanup
EOF

# 8. Systemd service für Watchdog
echo "🐕 Setting up application watchdog..."
sudo tee /etc/systemd/system/cmc-watchdog.service > /dev/null << 'EOF'
[Unit]
Description=CMC Application Watchdog
After=cmc-app.service

[Service]
Type=simple
ExecStart=/usr/local/bin/cmc-health-check
Restart=always
RestartSec=300
User=root

[Install]
WantedBy=multi-user.target
EOF

sudo tee /etc/systemd/system/cmc-watchdog.timer > /dev/null << 'EOF'
[Unit]
Description=Run CMC Watchdog every 5 minutes
Requires=cmc-watchdog.service

[Timer]
OnCalendar=*:0/5
Persistent=true

[Install]
WantedBy=timers.target
EOF

sudo systemctl daemon-reload
sudo systemctl enable cmc-watchdog.timer
sudo systemctl start cmc-watchdog.timer

echo ""
echo "✅ Monitoring setup completed!"
echo ""
echo "📊 Monitoring Features:"
echo "   • Health checks every 5 minutes"
echo "   • System metrics collection every minute"
echo "   • Automatic log rotation (30 days)"
echo "   • Weekly cleanup of old files"
echo "   • Application watchdog with auto-restart"
echo ""
echo "📋 Check logs:"
echo "   tail -f /var/log/cmc-app/health-check.log"
echo "   tail -f /var/log/cmc-app/metrics.log"
echo ""
echo "⚙️  Manage watchdog:"
echo "   sudo systemctl status cmc-watchdog.timer"
echo "   sudo systemctl stop cmc-watchdog.timer"
