alter table [Question]
add ApprovalDate datetime null;

alter table [Question]
add RejectionDate datetime null;

alter table [Answer]
add ApprovalDate datetime null;

alter table [Answer]
add RejectionDate datetime null;
go;

update [Question] set ApprovalDate = GETDATE()
update [Answer] set ApprovalDate = GETDATE()