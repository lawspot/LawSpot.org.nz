ALTER TABLE Answer
ADD PublisherId INT NULL CONSTRAINT FK_Answer_PublisherId FOREIGN KEY REFERENCES Publisher(PublisherId)
GO

UPDATE Answer
SET PublisherId = 1
WHERE Approved = 1