/****** Object:  Table [dbo].[Lawyer]    Script Date: 2/12/2013 9:15:03 p.m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [Question] ADD
	[Status] int NULL

GO

UPDATE [Question]
SET [Status] = CASE WHEN Approved = 1 THEN 1 ELSE (CASE WHEN ReviewDate IS NOT NULL THEN 2 ELSE 0 END) END
GO

ALTER TABLE [Question]
ALTER COLUMN [Status] INT NOT NULL