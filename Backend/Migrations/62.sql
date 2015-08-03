/****** Object:  Table [dbo].[Lawyer]    Script Date: 2/12/2013 9:15:03 p.m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [User]
ADD CanPublish BIT NULL
GO

UPDATE [User]
SET CanPublish = 0
WHERE PublisherId IS NULL

UPDATE [User]
SET CanPublish = 1
WHERE PublisherId IS NOT NULL

ALTER TABLE [User]
ALTER COLUMN CanPublish BIT NOT NULL