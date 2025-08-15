#!/bin/bash

# CMC Server Setup Script für Ubuntu 24
# Führt automatische Installation aller Dependencies durch

set -e

echo "🚀 Starting CMC Server Setup on Ubuntu 24..."

# System Update
echo "📦 Updating system packages..."
sudo apt update && sudo apt upgrade -y

# Docker Installation
echo "🐳 Installing Docker..."
if ! command -v docker &> /dev/null; then
    # Remove old versions
    sudo apt-get remove -y docker docker-engine docker.io containerd runc || true

    # Install dependencies
    sudo apt-get install -y \
        ca-certificates \
        curl \
        gnupg \
        lsb-release

    # Add Docker's official GPG key
    sudo mkdir -p /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

    # Set up repository
    echo \
      "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
      $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

    # Install Docker
    sudo apt-get update
    sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

    # Start and enable Docker
    sudo systemctl start docker
    sudo systemctl enable docker

    # Add user to docker group
    sudo usermod -aG docker $USER

    echo "✅ Docker installed successfully"
else
    echo "ℹ️  Docker already installed"
fi

# .NET 8.0 Installation
echo "🔧 Installing .NET 8.0..."
if ! command -v dotnet &> /dev/null; then
    # Microsoft package repository
    wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    sudo dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb

    # Install .NET 8.0
    sudo apt-get update
    sudo apt-get install -y dotnet-sdk-8.0 aspnetcore-runtime-8.0

    echo "✅ .NET 8.0 installed successfully"
else
    echo "ℹ️  .NET already installed"
fi

# Nginx Installation (Reverse Proxy für Port 80)
echo "🌐 Installing Nginx..."
if ! command -v nginx &> /dev/null; then
    sudo apt install -y nginx
    sudo systemctl start nginx
    sudo systemctl enable nginx
    echo "✅ Nginx installed successfully"
else
    echo "ℹ️  Nginx already installed"
fi

# Create application directory
echo "📁 Setting up application directories..."
sudo mkdir -p /var/www/cmc-app
sudo mkdir -p /var/log/cmc-app
sudo chown -R $USER:$USER /var/www/cmc-app
sudo chmod 755 /var/www/cmc-app

# Create systemd service directories
sudo mkdir -p /etc/systemd/system

echo "✅ Server setup completed!"
echo ""
echo "Next steps:"
echo "1. Logout and login again (for Docker group)"
echo "2. Deploy your application"
echo "3. Configure Nginx and systemd service"
