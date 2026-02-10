-- ============================================
-- This file contains SQL statements that will be executed after the build script.
-- Post‑Deployment Script: Ensure admin user exists
-- Works for SQL Server 2022 and Azure SQL Database
-- ============================================

DECLARE @Username NVARCHAR(100) = N'$(AdminUser)';
DECLARE @PasswordHash NVARCHAR(300) = N'$(AdminPasswordHash)';
-- The password is "TaskFlowAdmin". This is then hashed into a SHA-512 string followed by hashing using Argon2

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Users
    WHERE Username = @Username
)
BEGIN
    PRINT 'Creating admin user "' + @Username + '"...';

    INSERT INTO dbo.Users (Username, Password)
    VALUES (@Username, @PasswordHash);

    PRINT 'Admin user created.';
END
ELSE
BEGIN
    PRINT 'Admin user "' + @Username + '" already exists. Skipping.';
END;