#!/bin/bash

# Script sync code lên VPS (chỉ sync, không build)
# Sử dụng: ./sync-to-vps.sh [vps-ip] [username] [path]
# Ví dụ: ./sync-to-vps.sh 192.168.1.100 hanet /home/hanet/hanet-api

set -e

if [ -z "$1" ] || [ -z "$2" ]; then
    echo "Usage: $0 <vps-ip> <username> [remote-path]"
    echo "Example: $0 192.168.1.100 hanet /home/hanet/hanet-api"
    exit 1
fi

VPS_HOST="$1"
VPS_USER="$2"
VPS_PATH="${3:-/home/$VPS_USER/hanet-api}"

echo "=== Syncing to VPS ==="
echo "Target: $VPS_USER@$VPS_HOST:$VPS_PATH"
echo ""

# Tạo thư mục nếu chưa có
ssh $VPS_USER@$VPS_HOST "mkdir -p $VPS_PATH"

# Sync files
rsync -avz --progress \
    --exclude='bin/' \
    --exclude='obj/' \
    --exclude='publish/' \
    --exclude='.git/' \
    --exclude='.vs/' \
    --exclude='.vscode/' \
    --exclude='*.user' \
    --exclude='*.suo' \
    ./ $VPS_USER@$VPS_HOST:$VPS_PATH/

echo ""
echo "✓ Sync completed!"
echo ""
echo "Next steps on VPS:"
echo "  ssh $VPS_USER@$VPS_HOST"
echo "  cd $VPS_PATH"
echo "  ./build-on-vps.sh"
