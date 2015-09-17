ALTER TABLE Answer
ADD [Status] INT NULL
GO

UPDATE Answer
SET [Status] = 0

UPDATE Answer
SET [Status] = 1
WHERE Approved = 1

UPDATE Answer
SET [Status] = 2
WHERE Approved = 0 AND ReviewedByUserId IS NOT NULL

ALTER TABLE Answer
ALTER COLUMN [Status] INT NOT NULL

ALTER TABLE Answer
DROP COLUMN [Approved]