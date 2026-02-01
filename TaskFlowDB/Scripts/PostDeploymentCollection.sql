-- This file contains links to the SQL scripts that should be run during the post-deployment phase

-- Check for the presence of the default admin user
:r .\CreateFirstUser.sql

-- Ensure that the default task statuses exist and match the expected criteria
:r .\PopulateTaskStatus.sql