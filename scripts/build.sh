#!/bin/bash
# Build script for Monolith Update Site

set -e

echo "Building Monolith Firewall Update Site..."

# Navigate to source directory
cd "$(dirname "$0")/../src"

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Build the project
echo "Building project..."
dotnet build -c Release

echo "Build completed successfully!"
