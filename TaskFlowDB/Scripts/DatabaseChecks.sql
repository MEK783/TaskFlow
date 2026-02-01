-- ============================================
-- This file contains SQL statements that will be executed before the build script.
-- Pre-Deployment Script: Create database if missing
-- Works on SQL Server 2022 and Azure SQL Database
-- ============================================

DECLARE @DbName SYSNAME = N'$(DatabaseName)';

-- Detect platform
DECLARE @EngineEdition INT = CAST(SERVERPROPERTY('EngineEdition') AS INT);
-- 2 = SQL Server (on-prem) or Azure SQL Managed Instance
-- 5 = Azure SQL Database

IF @EngineEdition <> 5
BEGIN
    ------------------------------------------------------------
    -- On-prem SQL Server or Azure Managed Instance:
    -- CREATE DATABASE is allowed at server level
    ------------------------------------------------------------
    IF NOT EXISTS (
        SELECT 1
        FROM sys.databases
        WHERE name = @DbName
    )
    BEGIN
        PRINT 'Creating database "' + @DbName + '"...';
        DECLARE @Sql NVARCHAR(MAX) = N'CREATE DATABASE [' + @DbName + N']';
        EXEC (@Sql);
    END
    ELSE
    BEGIN
        PRINT 'Database "' + @DbName + '" already exists.';
    END
END
ELSE
BEGIN
    ------------------------------------------------------------
    -- Azure SQL Database:
    -- CREATE DATABASE is not permitted inside DB build scripts.
    -- Only conditionally check and warn.
    ------------------------------------------------------------
    PRINT 'Azure SQL Database detected.';
    PRINT 'CREATE DATABASE cannot be executed in this context.';

    -- Optional: Fail the deployment if the DB is missing
    -- IF DB_NAME() <> @DbName
    --     THROW 50001, ''Deployment must target the database "' + @DbName + '".'', 1;

END