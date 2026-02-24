#!/bin/bash

# Script build trên VPS (chạy script này trên VPS)
# Sử dụng: ./build-on-vps.sh

set -e

echo "=== Building Hanet API on VPS ==="

# Kiểm tra dotnet đã cài chưa
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK not found!"
    echo "Please install .NET 8 SDK first:"
    echo "  wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh"
    echo "  chmod +x dotnet-install.sh"
    echo "  ./dotnet-install.sh --channel 8.0"
    exit 1
fi

# Clean
echo "[1/4] Cleaning..."
dotnet clean Hanet.sln -c Release

# Restore
echo "[2/4] Restoring dependencies..."
dotnet restore Hanet.sln

# Build
echo "[3/4] Building..."
dotnet build Hanet.sln -c Release

# Publish
echo "[4/4] Publishing..."
dotnet publish Hanet.WebAPI/Hanet.WebAPI.csproj -c Release -o /var/www/hanet-api

echo "✓ Build completed!"
echo "Published to: /var/www/hanet-api"
echo ""
echo "Next steps:"
echo "  1. Configure appsettings: sudo nano /var/www/hanet-api/appsettings.json"
echo "  2. Setup systemd service: sudo ./setup-service.sh"
echo "  3. Start service: sudo systemctl start hanet-api"
