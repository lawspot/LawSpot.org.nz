alter table [Lawyer]
add Approved bit null;
go;

update [Lawyer] set Approved = 1

alter table [Lawyer]
alter column Approved bit not null;

alter table [User]
drop column IsLawyer;