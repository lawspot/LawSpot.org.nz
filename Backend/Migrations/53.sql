/****** Object:  Table [dbo].[Lawyer]    Script Date: 2/12/2013 9:15:03 p.m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [ConflictDeclaration]
(
DeclarationId INT NOT NULL IDENTITY,
UserId INT NOT NULL REFERENCES [User](UserId),
QuestionId INT NOT NULL REFERENCES [Question](QuestionId),
CONSTRAINT PK_ConflictDeclaration PRIMARY KEY CLUSTERED (DeclarationId)
)