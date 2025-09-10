#!/bin/bash

# Docker build script for ContentGenerator.API
# This script builds the Docker image with proper tagging

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
IMAGE_NAME="contentgenerator-api"
VERSION=${1:-"latest"}
REGISTRY=${2:-""}

echo -e "${GREEN}Building ContentGenerator.API Docker image...${NC}"

# Build the image
if [ -n "$REGISTRY" ]; then
    FULL_IMAGE_NAME="$REGISTRY/$IMAGE_NAME:$VERSION"
else
    FULL_IMAGE_NAME="$IMAGE_NAME:$VERSION"
fi

echo -e "${YELLOW}Building image: $FULL_IMAGE_NAME${NC}"

docker build -t "$FULL_IMAGE_NAME" .

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Docker image built successfully: $FULL_IMAGE_NAME${NC}"
    
    # Show image size
    echo -e "${YELLOW}Image size:${NC}"
    docker images "$FULL_IMAGE_NAME" --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}"
    
    # Optional: Push to registry
    if [ -n "$REGISTRY" ] && [ "$3" = "--push" ]; then
        echo -e "${YELLOW}Pushing image to registry...${NC}"
        docker push "$FULL_IMAGE_NAME"
        echo -e "${GREEN}✅ Image pushed successfully${NC}"
    fi
else
    echo -e "${RED}❌ Docker build failed${NC}"
    exit 1
fi
