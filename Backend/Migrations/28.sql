﻿ALTER TABLE Answer
ADD [References] NVARCHAR(MAX) NULL
GO

UPDATE Answer SET [References] = ''
GO

ALTER TABLE Answer
ALTER COLUMN [References] NVARCHAR(MAX) NOT NULL