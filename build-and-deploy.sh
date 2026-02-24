#!/bin/bash

# Script build và deploy lên VPS không dùng Docker
# Sử dụng: ./build-and-deploy.sh [vps-ip hoặc domain] [username] [path-on-vps]
# Ví dụ: ./build-and-deploy.sh 192.168.1.100 hanet /home/hanet/hanet-api

set -e

# Màu sắc cho output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Kiểm tra tham số
if [ -z "$1" ] || [ -z "$2" ]; then
    echo -e "${RED}Usage: $0 <vps-ip> <username> [remote-path]${NC}"
    echo "Example: $0 192.168.1.100 hanet /home/hanet/hanet-api"
    exit 1
fi

VPS_HOST="$1"
VPS_USER="$2"
VPS_PATH="${3:-/home/$VPS_USER/hanet-api}"
LOCAL_BUILD_PATH="./Hanet.WebAPI/bin/Release/net8.0/publish"

echo -e "${YELLOW}=== Hanet API Deployment Script ===${NC}"
echo "VPS: $VPS_USER@$VPS_HOST"
echo "Remote path: $VPS_PATH"
echo ""

# Step 1: Clean previous build
echo -e "${YELLOW}[1/5] Cleaning previous build...${NC}"
rm -rf ./Hanet.WebAPI/bin/Release
rm -rf ./Hanet.WebAPI/obj/Release
echo -e "${GREEN}✓ Cleaned${NC}"

# Step 2: Restore dependencies
echo -e "${YELLOW}[2/5] Restoring dependencies...${NC}"
dotnet restore Hanet.sln
echo -e "${GREEN}✓ Dependencies restored${NC}"

# Step 3: Build project
echo -e "${YELLOW}[3/5] Building project...${NC}"
dotnet build Hanet.sln -c Release
echo -e "${GREEN}✓ Build completed${NC}"

# Step 4: Publish
echo -e "${YELLOW}[4/5] Publishing application...${NC}"
dotnet publish Hanet.WebAPI/Hanet.WebAPI.csproj -c Release -o $LOCAL_BUILD_PATH
echo -e "${GREEN}✓ Published to $LOCAL_BUILD_PATH${NC}"

# Step 5: Deploy to VPS
echo -e "${YELLOW}[5/5] Deploying to VPS...${NC}"

# Tạo thư mục trên VPS nếu chưa có
ssh $VPS_USER@$VPS_HOST "mkdir -p $VPS_PATH"

# Stop service trước khi deploy (nếu đang chạy)
echo "Stopping service on VPS..."
ssh $VPS_USER@$VPS_HOST "sudo systemctl stop hanet-api 2>/dev/null || true"

# Backup bản cũ
echo "Creating backup..."
ssh $VPS_USER@$VPS_HOST "if [ -d $VPS_PATH/current ]; then mv $VPS_PATH/current $VPS_PATH/backup-\$(date +%Y%m%d-%H%M%S); fi"

# Sync files lên VPS
echo "Syncing files to VPS..."
rsync -avz --delete \
    --exclude='*.pdb' \
    --exclude='*.Development.json' \
    $LOCAL_BUILD_PATH/ $VPS_USER@$VPS_HOST:$VPS_PATH/current/

# Copy appsettings.json nếu chưa có
echo "Checking configuration files..."
ssh $VPS_USER@$VPS_HOST "if [ ! -f $VPS_PATH/current/appsettings.Production.json ]; then cp $VPS_PATH/current/appsettings.json $VPS_PATH/current/appsettings.Production.json; fi"

# Restart service
echo "Starting service on VPS..."
ssh $VPS_USER@$VPS_HOST "sudo systemctl start hanet-api"

echo -e "${GREEN}✓ Deployment completed${NC}"
echo ""
echo -e "${GREEN}=== Deployment Summary ===${NC}"
echo "Local build: $LOCAL_BUILD_PATH"
echo "Remote path: $VPS_PATH/current"
echo ""
echo "Check status: ssh $VPS_USER@$VPS_HOST 'sudo systemctl status hanet-api'"
echo "View logs: ssh $VPS_USER@$VPS_HOST 'sudo journalctl -u hanet-api -f'"
echo ""
