/****** Object:  Table [dbo].[Lawyer]    Script Date: 2/12/2013 9:15:03 p.m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [User]
ADD [HasPractisingAuthority] BIT NULL
GO

UPDATE [User]
SET [HasPractisingAuthority] = 0
WHERE PublisherId IS NULL

UPDATE [User]
SET [HasPractisingAuthority] = 1
WHERE PublisherId IS NOT NULL

ALTER TABLE [User]
ALTER COLUMN [HasPractisingAuthority] BIT NOT NULL