/****** Object:  Table [dbo].[Lawyer]    Script Date: 2/12/2013 9:15:03 p.m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [Publisher]
DROP COLUMN [Logo]

ALTER TABLE [Publisher]
ADD [EmailAddress] NVARCHAR(256)

ALTER TABLE [Publisher]
ADD [PhoneNumber] NVARCHAR(50)

ALTER TABLE [Publisher]
ADD [WebsiteUri] NVARCHAR(256)

ALTER TABLE [Publisher]
ADD [PhysicalAddress] NVARCHAR(256)