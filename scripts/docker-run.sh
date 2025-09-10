#!/bin/bash

# Docker run script for ContentGenerator.API
# This script provides easy commands to run the application

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
COMPOSE_FILE="docker-compose.yml"
COMPOSE_OVERRIDE="docker-compose.override.yml"
COMPOSE_PROD="docker-compose.prod.yml"

show_help() {
    echo -e "${BLUE}ContentGenerator.API Docker Management Script${NC}"
    echo ""
    echo "Usage: $0 [COMMAND]"
    echo ""
    echo "Commands:"
    echo "  dev         Start development environment"
    echo "  prod        Start production environment"
    echo "  stop        Stop all services"
    echo "  restart     Restart all services"
    echo "  logs        Show logs for all services"
    echo "  logs-api    Show logs for API service only"
    echo "  build       Build all services"
    echo "  clean       Clean up containers and volumes"
    echo "  migrate     Run database migrations"
    echo "  shell       Open shell in API container"
    echo "  status      Show status of all services"
    echo "  help        Show this help message"
    echo ""
}

start_dev() {
    echo -e "${GREEN}Starting development environment...${NC}"
    docker-compose -f "$COMPOSE_FILE" -f "$COMPOSE_OVERRIDE" up -d
    echo -e "${GREEN}✅ Development environment started${NC}"
    echo -e "${YELLOW}API available at: http://localhost:5000${NC}"
    echo -e "${YELLOW}Swagger UI: http://localhost:5000/swagger${NC}"
}

start_prod() {
    echo -e "${GREEN}Starting production environment...${NC}"
    docker-compose -f "$COMPOSE_FILE" -f "$COMPOSE_PROD" up -d
    echo -e "${GREEN}✅ Production environment started${NC}"
}

stop_services() {
    echo -e "${YELLOW}Stopping all services...${NC}"
    docker-compose -f "$COMPOSE_FILE" -f "$COMPOSE_OVERRIDE" down
    echo -e "${GREEN}✅ All services stopped${NC}"
}

restart_services() {
    echo -e "${YELLOW}Restarting all services...${NC}"
    docker-compose -f "$COMPOSE_FILE" -f "$COMPOSE_OVERRIDE" restart
    echo -e "${GREEN}✅ All services restarted${NC}"
}

show_logs() {
    echo -e "${BLUE}Showing logs for all services...${NC}"
    docker-compose -f "$COMPOSE_FILE" -f "$COMPOSE_OVERRIDE" logs -f
}

show_api_logs() {
    echo -e "${BLUE}Showing logs for API service...${NC}"
    docker-compose -f "$COMPOSE_FILE" -f "$COMPOSE_OVERRIDE" logs -f api
}

build_services() {
    echo -e "${GREEN}Building all services...${NC}"
    docker-compose -f "$COMPOSE_FILE" -f "$COMPOSE_OVERRIDE" build
    echo -e "${GREEN}✅ All services built${NC}"
}

clean_up() {
    echo -e "${YELLOW}Cleaning up containers and volumes...${NC}"
    docker-compose -f "$COMPOSE_FILE" -f "$COMPOSE_OVERRIDE" down -v --remove-orphans
    docker system prune -f
    echo -e "${GREEN}✅ Cleanup completed${NC}"
}

run_migrations() {
    echo -e "${GREEN}Running database migrations...${NC}"
    docker-compose -f "$COMPOSE_FILE" -f "$COMPOSE_OVERRIDE" run --rm migration
    echo -e "${GREEN}✅ Migrations completed${NC}"
}

open_shell() {
    echo -e "${BLUE}Opening shell in API container...${NC}"
    docker-compose -f "$COMPOSE_FILE" -f "$COMPOSE_OVERRIDE" exec api /bin/bash
}

show_status() {
    echo -e "${BLUE}Service Status:${NC}"
    docker-compose -f "$COMPOSE_FILE" -f "$COMPOSE_OVERRIDE" ps
}

# Main script logic
case "${1:-help}" in
    dev)
        start_dev
        ;;
    prod)
        start_prod
        ;;
    stop)
        stop_services
        ;;
    restart)
        restart_services
        ;;
    logs)
        show_logs
        ;;
    logs-api)
        show_api_logs
        ;;
    build)
        build_services
        ;;
    clean)
        clean_up
        ;;
    migrate)
        run_migrations
        ;;
    shell)
        open_shell
        ;;
    status)
        show_status
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        echo -e "${RED}Unknown command: $1${NC}"
        show_help
        exit 1
        ;;
esac
