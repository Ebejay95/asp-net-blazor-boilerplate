#!/bin/bash

echo "🔍 pgAdmin Debug Diagnose"
echo "========================="

echo ""
echo "1️⃣ Laufende Container:"
docker ps --format "table {{.Names}}\t{{.Image}}\t{{.Status}}\t{{.Ports}}"

echo ""
echo "2️⃣ pgAdmin Container Status:"
if docker ps | grep -q pgadmin; then
    echo "✅ pgAdmin Container läuft"

    echo ""
    echo "3️⃣ pgAdmin Logs (letzte 20 Zeilen):"
    docker logs pgadmin-cmc --tail=20

    echo ""
    echo "4️⃣ pgAdmin Port-Mapping:"
    docker port pgadmin-cmc

    echo ""
    echo "5️⃣ pgAdmin Container Details:"
    docker inspect pgadmin-cmc | grep -A 10 -B 5 "IPAddress\|PortBindings"

else
    echo "❌ pgAdmin Container läuft nicht!"

    echo ""
    echo "📋 Alle Container (auch gestoppte):"
    docker ps -a | grep pgadmin

    echo ""
    echo "📄 pgAdmin Container Logs (falls vorhanden):"
    docker logs pgadmin-cmc 2>/dev/null || echo "Keine Logs verfügbar"
fi

echo ""
echo "6️⃣ Docker Compose Status:"
docker-compose ps

echo ""
echo "7️⃣ Netzwerk-Test von Host:"
echo "Teste localhost:8080..."
curl -I http://localhost:8080 2>/dev/null | head -n 1 || echo "❌ Keine Antwort von localhost:8080"

echo ""
echo "8️⃣ Überprüfe Docker-Konfigurationsdateien:"
echo "docker/pgadmin-servers.json:"
if [ -f docker/pgadmin-servers.json ]; then
    echo "✅ Datei existiert"
    cat docker/pgadmin-servers.json
else
    echo "❌ Datei fehlt"
fi

echo ""
echo "9️⃣ Empfohlene Fixes:"
echo "   - make docker-down && make docker-dev"
echo "   - docker-compose logs pgadmin"
echo "   - docker restart pgadmin-cmc"
