CREATE TABLE [dbo].[TaskStatus]
(
  Id INT IDENTITY(1, 1) NOT NULL,
  StatusCode NVARCHAR(50) NOT NULL,
  StatusDescription NVARCHAR(200) NULL,
  --Holds the definition of the exact icon to import using lazy loading. Value should be <library>/<icon>
  ReactIcon NVARCHAR(50) NOT NULL CONSTRAINT DF_TaskStatus_ReactIcon DEFAULT (''),
  ClosingStatus BIT NOT NULL CONSTRAINT DF_TaskStatus_ClosingStatus DEFAULT (0),
  CONSTRAINT PK_TaskStatus PRIMARY KEY (Id),
  CONSTRAINT CK_TaskStatus_ReactIcon CHECK (
    LEN(ReactIcon) = 0
    OR (
      ReactIcon COLLATE Latin1_General_100_CS_AS LIKE '[a-z][a-z]/%'
      AND LEN(ReactIcon) > 3
      AND SUBSTRING(ReactIcon, 4, LEN(ReactIcon) - 3) NOT LIKE '%[^A-Za-z0-9]%'
    )
  )
)
GO