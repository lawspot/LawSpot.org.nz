alter table Question
add ReviewDate datetimeoffset null;

alter table Question
add ReviewedByUserId int null;

alter table Answer
add ReviewDate datetimeoffset null;

alter table Answer
add ReviewedByUserId int null;
go;


update Question set
ReviewDate = COALESCE(ApprovalDate, RejectionDate),
ReviewedByUserId = COALESCE(ApprovedByUserId, RejectedByUserId)

update Answer set
ReviewDate = COALESCE(ApprovalDate, RejectionDate),
ReviewedByUserId = COALESCE(ApprovedByUserId, RejectedByUserId)

alter table Question
drop column ApprovalDate;

alter table Question
drop column RejectionDate;

alter table Question
drop column ApprovedByUserId;

alter table Question
drop column RejectedByUserId;

alter table Answer
drop column ApprovalDate;

alter table Answer
drop column RejectionDate;

alter table Answer
drop column ApprovedByUserId;

alter table Answer
drop column RejectedByUserId;