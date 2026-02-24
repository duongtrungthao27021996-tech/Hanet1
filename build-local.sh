#!/bin/bash

# Script build project local
# Sử dụng: ./build-local.sh

set -e

echo "=== Building Hanet API ==="

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
dotnet publish Hanet.WebAPI/Hanet.WebAPI.csproj -c Release -o ./publish

echo "✓ Build completed successfully!"
echo "Output: ./publish"
echo ""
echo "To run locally: cd publish && dotnet Hanet.WebAPI.dll"
