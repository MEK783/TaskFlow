CREATE TABLE [dbo].[Tasks]
(
  Id INT IDENTITY(1, 1) NOT NULL,
  TaskName NVARCHAR(100) NOT NULL CONSTRAINT DF_TaskName DEFAULT (''),
  TaskDescription NVARCHAR(MAX) NULL,
  Status_TaskStatusFK INT NOT NULL,
  CreatedBy_UserFK INT NOT NULL,
  CreatedOn DATETIME NOT NULL CONSTRAINT DF_CreatedOn DEFAULT (GETDATE()),
  StatusPriority INT NOT NULL CONSTRAINT DF_StatusPriority DEFAULT (0),
  ModifiedOn DATETIME NOT NULL CONSTRAINT DF_ModifiedOn DEFAULT (GETDATE()),
  ClosedOn DATETIME NULL,
  CONSTRAINT PK_Tasks PRIMARY KEY (Id),
  CONSTRAINT FK_Tasks_TaskStatus FOREIGN KEY (Status_TaskStatusFK) REFERENCES TaskStatus(Id),
  CONSTRAINT FK_Tasks_Users FOREIGN KEY (CreatedBy_UserFK) REFERENCES Users(Id),
  CONSTRAINT UQ_Tasks_TaskName UNIQUE (TaskName, CreatedBy_UserFK),
  CONSTRAINT UQ_Tasks_StatusPriority UNIQUE (Status_TaskStatusFK, CreatedBy_UserFK, StatusPriority)
)
GO

CREATE INDEX CK_Tasks_StatusPriority ON Tasks(Status_TaskStatusFK, CreatedBy_UserFK, StatusPriority)
GO