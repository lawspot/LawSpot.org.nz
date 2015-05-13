/****** Object:  Table [dbo].[Lawyer]    Script Date: 2/12/2013 9:15:03 p.m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [PublisherCategory]
(
PublisherId INT NOT NULL REFERENCES [Publisher](PublisherId),
CategoryId INT NOT NULL REFERENCES [Category](CategoryId)
)

CREATE UNIQUE NONCLUSTERED INDEX UK_PublisherCategory ON dbo.[PublisherCategory] (PublisherId, CategoryId);