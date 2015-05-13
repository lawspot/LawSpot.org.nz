/****** Object:  Table [dbo].[Lawyer]    Script Date: 2/12/2013 9:15:03 p.m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [Publisher]
ADD [Description] NVARCHAR(150)

ALTER TABLE [Publisher]
ADD [Logo] VARBINARY(MAX)