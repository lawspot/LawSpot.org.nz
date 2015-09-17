alter table [Question] add ViewCount int null
go

update [Question] set ViewCount = 0

alter table [Question] alter column ViewCount int not null