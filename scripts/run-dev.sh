#!/bin/bash
# Development run script

set -e

echo "Starting Monolith Update Site in development mode..."

# Navigate to source directory
cd "$(dirname "$0")/../src"

# Run with hot reload
dotnet watch run
