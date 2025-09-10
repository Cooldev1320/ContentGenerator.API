-- Database initialization script for ContentGenerator
-- This script runs when the PostgreSQL container starts for the first time

-- Create database if it doesn't exist (this is handled by POSTGRES_DB env var)
-- CREATE DATABASE IF NOT EXISTS contentgenerator;

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create a dedicated user for the application (optional)
-- CREATE USER contentgenerator_user WITH PASSWORD 'secure_password_123';
-- GRANT ALL PRIVILEGES ON DATABASE contentgenerator TO contentgenerator_user;

-- Set timezone
SET timezone = 'UTC';

-- Create initial schema (this will be handled by Entity Framework migrations)
-- The actual tables will be created when the API runs EF migrations

-- Log the initialization
DO $$
BEGIN
    RAISE NOTICE 'ContentGenerator database initialized successfully';
END $$;
