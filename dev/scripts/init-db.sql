-- Database initialization script for 10xDev-TaskFlow
-- This script runs automatically when the PostgreSQL container is first created

-- The database '10xDev-TaskFlow' is already created by the POSTGRES_DB environment variable
-- This script can be used for additional initialization if needed

-- Create extensions if needed
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Grant privileges to postgres user (already the owner)
GRANT ALL PRIVILEGES ON DATABASE "10xDev-TaskFlow" TO postgres;

-- Log successful initialization
SELECT 'Database 10xDev-TaskFlow initialized successfully' AS status;
