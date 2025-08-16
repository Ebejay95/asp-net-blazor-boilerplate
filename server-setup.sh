#!/bin/bash
# CMC Server Setup Script für Ubuntu 24.04 LTS
# Installiert Docker, .NET 8, Nginx, Node 20 (für Tailwind v4) und legt einen Tailwind-Build-Helper an.

set -euo pipefail

echo "🚀 Starting CMC Server Setup on Ubuntu 24..."

# --- System Update ---
echo "📦 Updating system packages..."
sudo apt update
sudo apt upgrade -y

# --- Docker Installation ---
echo "🐳 Installing Docker..."
if ! command -v docker >/dev/null 2>&1; then
  # Alte Versionen entfernen (idempotent)
  sudo apt-get remove -y docker docker-engine docker.io containerd runc || true

  # Dependencies
  sudo apt-get install -y ca-certificates curl gnupg lsb-release

  # Docker Repo
  sudo mkdir -p /etc/apt/keyrings
  curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
  echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list >/dev/null

  sudo apt-get update
  sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

  # Start + Enable
  sudo systemctl enable --now docker

  # User in docker-Gruppe
  sudo usermod -aG docker "$USER" || true

  echo "✅ Docker installed successfully"
else
  echo "ℹ️  Docker already installed"
fi

# --- .NET 8 Installation ---
echo "🔧 Installing .NET 8.0..."
if ! command -v dotnet >/dev/null 2>&1; then
  wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb
  rm -f packages-microsoft-prod.deb

  sudo apt-get update
  sudo apt-get install -y dotnet-sdk-8.0 aspnetcore-runtime-8.0

  echo "✅ .NET 8.0 installed successfully"
else
  echo "ℹ️  .NET already installed"
fi

# --- Nginx Installation ---
echo "🌐 Installing Nginx..."
if ! command -v nginx >/dev/null 2>&1; then
  sudo apt-get install -y nginx
  sudo systemctl enable --now nginx
  echo "✅ Nginx installed successfully"
else
  echo "ℹ️  Nginx already installed"
fi

# --- Node.js 20 (für Tailwind v4 CLI) ---
echo "🧰 Installing Node.js 20 (for Tailwind CLI)..."
if ! command -v node >/dev/null 2>&1; then
  curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
  sudo apt-get install -y nodejs
  echo "✅ Node $(node -v) / npm $(npm -v) installed"
else
  echo "ℹ️  Node already installed: $(node -v); npm: $(npm -v)"
fi

# --- App-Verzeichnisse ---
echo "📁 Setting up application directories..."
sudo mkdir -p /var/www/cmc-app
sudo mkdir -p /var/log/cmc-app
sudo chown -R "$USER":"$USER" /var/www/cmc-app
sudo chmod 755 /var/www/cmc-app

# --- systemd Ordner sicherstellen ---
sudo mkdir -p /etc/systemd/system

# --- Tailwind Build Helper anlegen ---
# Dieser Helper baut deine CSS aus dem Repo-Pfad und bricht hart ab, wenn style.css leer wäre.
echo "🧵 Installing Tailwind build helper..."
sudo tee /usr/local/bin/cmc-tailwind-build >/dev/null <<'EOF'
#!/usr/bin/env bash
set -euo pipefail

APP_DIR="/var/www/cmc-app"
IN="$APP_DIR/src/CMC.Web/Styles/app.css"
OUT="$APP_DIR/src/CMC.Web/wwwroot/style.css"

if [ ! -f "$IN" ]; then
  echo "❌ Tailwind input not found: $IN"
  exit 1
fi

echo "🏗️  Tailwind build (minify)…"
cd "$APP_DIR"
npx @tailwindcss/cli@latest -i "$IN" -o "$OUT" --minify

# Sanity check: Datei muss Bytes enthalten
if [ ! -s "$OUT" ]; then
  echo "❌ style.css ist leer – check @import/@source Pfade und Tailwind-Konfiguration"
  exit 2
fi

echo "✅ Tailwind built: $(wc -c < "$OUT") bytes -> $OUT"
EOF
sudo chmod +x /usr/local/bin/cmc-tailwind-build
echo "✅ Tailwind helper installed at /usr/local/bin/cmc-tailwind-build"

echo "✅ Server setup completed!"
echo ""
echo "Next steps:"
echo "1) Logout/Login (damit die docker-Gruppe für $USER aktiv wird)"
echo "2) Deployment ausführen (Code + publish + systemd/Nginx)"
echo "3) Im Deploy-Job 'cmc-tailwind-build' vor 'dotnet publish' aufrufen"
