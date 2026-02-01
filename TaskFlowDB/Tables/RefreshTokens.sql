CREATE TABLE [dbo].[RefreshTokens]
(
  Id INT IDENTITY(1,1) NOT NULL,
  Token NVARCHAR(128) NOT NULL,
  For_UsersFK INT NOT NULL,
  CreatedOn DATETIME NOT NULL CONSTRAINT DF_RefreshTokens_CreatedOn DEFAULT (GETDATE()),
  ExpiresOn DATETIME NOT NULL,
  RevokedOn DATETIME NULL,
  ReplacingToken NVARCHAR(128) NULL,
  CONSTRAINT PK_RefreshTokens PRIMARY KEY (Id),
  CONSTRAINT UQ_RefreshTokens_Token UNIQUE (Token),
  CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (For_UsersFK) REFERENCES Users(Id)
)
GO

CREATE UNIQUE INDEX UX_RefreshTokens_ActiveTokens ON RefreshTokens(For_UsersFK, Token)
WHERE RevokedOn IS NULL;
GO