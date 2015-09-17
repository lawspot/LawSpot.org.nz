alter table [Lawyer]
add CreatedOn datetime null;
go;

update [Lawyer] set CreatedOn = GETDATE()

alter table [Lawyer]
alter column CreatedOn datetime not null;