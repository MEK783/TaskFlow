CREATE TABLE [dbo].[Invites]
(
  Id INT IDENTITY(1, 1) NOT NULL,
  InviteCode NVARCHAR(16) NOT NULL,
  CreatedBy_UserFK INT NOT NULL,
  CreatedOn DATETIME NOT NULL CONSTRAINT DF_Invites_CreatedOn DEFAULT (GETDATE()),
  ExpiresOn DATETIME NOT NULL CONSTRAINT DF_Invites_ExpiresOn DEFAULT (DATEADD(DD, 15, GETDATE())),
  UsedOn DATETIME NULL,
  UsedBy_UserFK INT NULL,
  CONSTRAINT PK_Invites PRIMARY KEY (Id),
  CONSTRAINT UQ_Invites_InviteCode UNIQUE (InviteCode),
  CONSTRAINT FK_Invites_CreatedBy_Users FOREIGN KEY (CreatedBy_UserFK) REFERENCES Users(Id),
  CONSTRAINT FK_Invites_UsedBy_Users FOREIGN KEY (UsedBy_UserFK) REFERENCES Users(Id)
)
GO

CREATE UNIQUE INDEX UX_Invites_InviteCode ON Invites(InviteCode)
INCLUDE (ExpiresOn, UsedOn)
GO