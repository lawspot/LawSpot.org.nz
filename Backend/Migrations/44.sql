CREATE TABLE Publisher (
	PublisherId INT NOT NULL IDENTITY CONSTRAINT PK_Publisher PRIMARY KEY,
	Name NVARCHAR(50) NOT NULL
)
GO

INSERT Publisher (Name) VALUES ('Community Law Wellington & Hutt Valley')

ALTER TABLE [User]
ADD PublisherId INT NULL CONSTRAINT FK_User_PublisherId FOREIGN KEY REFERENCES Publisher(PublisherId)
GO

UPDATE [User]
SET PublisherId = 1
WHERE EmailAddress LIKE ('%@wclc.org.nz')