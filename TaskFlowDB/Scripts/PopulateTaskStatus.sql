-- ============================================
-- This file contains SQL statements that will be executed after the build script.
-- Post‑Deployment Script: Ensure admin user exists
-- Works for SQL Server 2022 and Azure SQL Database
-- ============================================

MERGE TaskStatus DEST
USING (
    SELECT
        *
    FROM
        (
            VALUES
            ('To Do', 'Tasks waiting to be started', 'md/MdOutlineCreate', CONVERT(BIT, 0)),
            ('In Progress', 'Currently ongoing tasks', 'md/MdAutorenew', CONVERT(BIT, 0)),
            ('Done', 'Finished tasks', 'md/MdChecklist', CONVERT(BIT, 1))
        ) VALS(Code, Description, Icon, Closer)
) SRC
    ON DEST.StatusCode = SRC.Code
WHEN MATCHED AND (
        DEST.StatusDescription <> SRC.Description
        OR DEST.ReactIcon <> SRC.Icon
        OR DEST.ClosingStatus <> SRC.Closer
    ) THEN
UPDATE SET
    DEST.StatusDescription = SRC.Description,
    DEST.ReactIcon = SRC.Icon,
    DEST.ClosingStatus = SRC.Closer
WHEN NOT MATCHED BY TARGET THEN
INSERT (
    StatusCode,
    StatusDescription,
    ReactIcon,
    ClosingStatus
)
VALUES (
    SRC.Code,
    SRC.Description,
    SRC.Icon,
    SRC.Closer
);
