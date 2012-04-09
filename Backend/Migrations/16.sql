alter table [Question]
add ApprovedByUserId int null;

alter table [Question]
add RejectedByUserId int null;

alter table [Answer]
add ApprovedByUserId int null;

alter table [Answer]
add RejectedByUserId int null;
go;

update [Question] set ApprovedByUserId = 1
update [Answer] set ApprovedByUserId = 1