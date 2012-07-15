ALTER TABLE [User]
ADD CanAdminister BIT NULL
GO

UPDATE [User]
SET CanAdminister = (CASE WHEN EmailAddress = 'paulbartrum@hotmail.com' THEN 1 ELSE 0 END)

ALTER TABLE [User]
ALTER COLUMN CanAdminister BIT NOT NULL