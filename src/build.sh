# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#!/bin/bash

# Get the directory where the script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_FILE="$SCRIPT_DIR/AzureMCP.csproj"

# Create .dist directory if it doesn't exist
mkdir -p "$SCRIPT_DIR/.dist"

# Clean previous builds
rm -rf "$SCRIPT_DIR/.dist"/*

# Determine the runtime based on OS and architecture
if [[ "$(uname)" == "Darwin" ]]; then
    if [[ "$(uname -m)" == "arm64" ]]; then
        RUNTIME="osx-arm64"
    else
        RUNTIME="osx-x64"
    fi
else
    if [[ "$(uname -m)" == "arm64" ]]; then
        RUNTIME="linux-arm64"
    else
        RUNTIME="linux-x64"
    fi
fi

# Build the project
echo -e "\033[32mBuilding azmcp for ${RUNTIME}...\033[0m"
dotnet publish "$PROJECT_FILE" --runtime "$RUNTIME" --self-contained --output .dist 

if [ $? -eq 0 ]; then
    echo -e "\n\033[32mBuild completed successfully!\033[0m"
    echo -e "\033[33mBinary location: $(realpath .dist/azmcp)\033[0m"
else
    echo -e "\n\033[31mBuild failed!\033[0m"
    exit 1
fi