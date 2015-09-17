﻿CREATE TABLE EventStream (
	EventId INT NOT NULL IDENTITY CONSTRAINT PK_EventStream PRIMARY KEY,
	EventDate DATETIMEOFFSET NOT NULL,
	EventType INT NOT NULL,
	UserId INT NOT NULL CONSTRAINT FK_EventStream_UserId FOREIGN KEY REFERENCES [User](UserId),
	Details NVARCHAR(MAX) NULL
)

CREATE NONCLUSTERED INDEX IX_EventStream_EventDate ON dbo.EventStream (EventDate);
CREATE NONCLUSTERED INDEX IX_EventStream_UserId ON dbo.EventStream (UserId);