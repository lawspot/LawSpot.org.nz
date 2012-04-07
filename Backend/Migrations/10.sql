DROP INDEX UK_User_EmailAddress ON dbo.[User];

alter table [User]
alter column EmailAddress nvarchar(256) not null;

CREATE UNIQUE NONCLUSTERED INDEX UK_User_EmailAddress ON dbo.[User] (EmailAddress);