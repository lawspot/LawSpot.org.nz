alter table [Question]
add Approved bit null;

alter table [Answer]
add Approved bit null;
go;

update [Question] set Approved = 1
update [Answer] set Approved = 1

alter table [Question]
alter column Approved bit not null;

alter table [Answer]
alter column Approved bit not null;