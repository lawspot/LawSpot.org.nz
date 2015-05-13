/****** Object:  Table [dbo].[Lawyer]    Script Date: 2/12/2013 9:15:03 p.m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [Publisher]
DROP COLUMN [Description]

ALTER TABLE [Publisher]
ADD [ShortDescription] NVARCHAR(250)

ALTER TABLE [Publisher]
ADD [LongDescription] NVARCHAR(MAX)