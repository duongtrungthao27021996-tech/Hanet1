#!/bin/bash

# Script tạo systemd service trên VPS (chạy với sudo)
# Sử dụng: sudo ./setup-service.sh [username] [port]

set -e

SERVICE_USER="${1:-$USER}"
SERVICE_PORT="${2:-5000}"
DOTNET_PATH=$(which dotnet || echo "$HOME/.dotnet/dotnet")
APP_PATH="/var/www/hanet-api"

echo "=== Setting up Hanet API Service ==="
echo "User: $SERVICE_USER"
echo "Port: $SERVICE_PORT"
echo "Path: $APP_PATH"
echo ""

# Tạo systemd service file
cat > /etc/systemd/system/hanet-api.service << EOF
[Unit]
Description=Hanet API Service
After=network.target

[Service]
Type=notify
User=$SERVICE_USER
WorkingDirectory=$APP_PATH
ExecStart=$DOTNET_PATH $APP_PATH/Hanet.WebAPI.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=hanet-api
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:$SERVICE_PORT
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
EOF

echo "✓ Service file created at /etc/systemd/system/hanet-api.service"

# Reload systemd
systemctl daemon-reload
echo "✓ Systemd reloaded"

# Enable service
systemctl enable hanet-api
echo "✓ Service enabled (auto-start on boot)"

echo ""
echo "=== Service Setup Complete ==="
echo ""
echo "Manage service:"
echo "  Start:   sudo systemctl start hanet-api"
echo "  Stop:    sudo systemctl stop hanet-api"
echo "  Restart: sudo systemctl restart hanet-api"
echo "  Status:  sudo systemctl status hanet-api"
echo "  Logs:    sudo journalctl -u hanet-api -f"
echo ""
