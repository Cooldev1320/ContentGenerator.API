@echo off
REM Docker run script for ContentGenerator.API (Windows)
REM This script provides easy commands to run the application

setlocal enabledelayedexpansion

REM Configuration
set COMPOSE_FILE=docker-compose.yml
set COMPOSE_OVERRIDE=docker-compose.override.yml
set COMPOSE_PROD=docker-compose.prod.yml

if "%1"=="help" goto :show_help
if "%1"=="--help" goto :show_help
if "%1"=="-h" goto :show_help
if "%1"=="" goto :show_help

if "%1"=="dev" goto :start_dev
if "%1"=="prod" goto :start_prod
if "%1"=="stop" goto :stop_services
if "%1"=="restart" goto :restart_services
if "%1"=="logs" goto :show_logs
if "%1"=="logs-api" goto :show_api_logs
if "%1"=="build" goto :build_services
if "%1"=="clean" goto :clean_up
if "%1"=="migrate" goto :run_migrations
if "%1"=="shell" goto :open_shell
if "%1"=="status" goto :show_status

echo Unknown command: %1
goto :show_help

:show_help
echo ContentGenerator.API Docker Management Script
echo.
echo Usage: %0 [COMMAND]
echo.
echo Commands:
echo   dev         Start development environment
echo   prod        Start production environment
echo   stop        Stop all services
echo   restart     Restart all services
echo   logs        Show logs for all services
echo   logs-api    Show logs for API service only
echo   build       Build all services
echo   clean       Clean up containers and volumes
echo   migrate     Run database migrations
echo   shell       Open shell in API container
echo   status      Show status of all services
echo   help        Show this help message
echo.
goto :end

:start_dev
echo Starting development environment...
docker-compose -f %COMPOSE_FILE% -f %COMPOSE_OVERRIDE% up -d
echo Development environment started
echo API available at: http://localhost:5000
echo Swagger UI: http://localhost:5000/swagger
goto :end

:start_prod
echo Starting production environment...
docker-compose -f %COMPOSE_FILE% -f %COMPOSE_PROD% up -d
echo Production environment started
goto :end

:stop_services
echo Stopping all services...
docker-compose -f %COMPOSE_FILE% -f %COMPOSE_OVERRIDE% down
echo All services stopped
goto :end

:restart_services
echo Restarting all services...
docker-compose -f %COMPOSE_FILE% -f %COMPOSE_OVERRIDE% restart
echo All services restarted
goto :end

:show_logs
echo Showing logs for all services...
docker-compose -f %COMPOSE_FILE% -f %COMPOSE_OVERRIDE% logs -f
goto :end

:show_api_logs
echo Showing logs for API service...
docker-compose -f %COMPOSE_FILE% -f %COMPOSE_OVERRIDE% logs -f api
goto :end

:build_services
echo Building all services...
docker-compose -f %COMPOSE_FILE% -f %COMPOSE_OVERRIDE% build
echo All services built
goto :end

:clean_up
echo Cleaning up containers and volumes...
docker-compose -f %COMPOSE_FILE% -f %COMPOSE_OVERRIDE% down -v --remove-orphans
docker system prune -f
echo Cleanup completed
goto :end

:run_migrations
echo Running database migrations...
docker-compose -f %COMPOSE_FILE% -f %COMPOSE_OVERRIDE% run --rm migration
echo Migrations completed
goto :end

:open_shell
echo Opening shell in API container...
docker-compose -f %COMPOSE_FILE% -f %COMPOSE_OVERRIDE% exec api /bin/bash
goto :end

:show_status
echo Service Status:
docker-compose -f %COMPOSE_FILE% -f %COMPOSE_OVERRIDE% ps
goto :end

:end
endlocal
