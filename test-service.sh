#!/bin/bash
# Script để test service trên VPS
# Upload và chạy trên VPS: bash test-service.sh

echo "=== Testing Hanet API Service ==="

echo -e "\n1. Checking dotnet runtime..."
/usr/share/dotnet/dotnet --list-runtimes

echo -e "\n2. Checking application files..."
ls -lh /var/www/hanet-api/current/Hanet.WebAPI.dll

echo -e "\n3. Checking appsettings.Production.json..."
cat /var/www/hanet-api/current/appsettings.Production.json

echo -e "\n4. Testing application manually (will timeout after 5 seconds)..."
cd /var/www/hanet-api/current
timeout 5 /usr/share/dotnet/dotnet Hanet.WebAPI.dll 2>&1 || echo "App started (killed by timeout - this is OK)"

echo -e "\n5. Checking service file..."
cat /etc/systemd/system/hanet-api.service

echo -e "\n6. Checking service status..."
systemctl status hanet-api.service --no-pager -l

echo -e "\n7. Checking recent logs..."
journalctl -u hanet-api.service -n 20 --no-pager

echo -e "\n=== Test completed ==="
