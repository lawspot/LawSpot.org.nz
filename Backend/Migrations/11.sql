alter table [User]
add CreatedOn datetime null;
go;

update [User] set CreatedOn = GETDATE()

alter table [User]
alter column CreatedOn datetime not null;