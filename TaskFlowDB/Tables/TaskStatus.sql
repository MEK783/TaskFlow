CREATE TABLE [dbo].[TaskStatus]
(
  Id INT IDENTITY(1, 1) NOT NULL,
  StatusCode NVARCHAR(50) NOT NULL,
  StatusDescription NVARCHAR(200) NULL,
  --Holds the definition of the exact icon to import using lazy loading. Value should be <library>/<icon>
  ReactIcon NVARCHAR(50) NOT NULL CONSTRAINT DF_TaskStatus_ReactIcon DEFAULT (''),
  ClosingStatus BIT NOT NULL CONSTRAINT DF_TaskStatus_ClosingStatus DEFAULT (0),
  CONSTRAINT PK_TaskStatus PRIMARY KEY (Id),
  CONSTRAINT CK_TaskStatus_ReactIcon CHECK (LEN(ReactIcon) = 0 OR REGEXP_LIKE(ReactIcon, '^(?i)([A-Za-z]{2})/\1[A-Za-z0-9]+$'))
)
GO