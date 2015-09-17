alter table Lawyer
add ReviewDate datetimeoffset null;

alter table Lawyer
add ReviewedByUserId int null;
go;


update Lawyer set
ReviewDate = COALESCE(ApprovalDate, RejectionDate),
ReviewedByUserId = COALESCE(ApprovedByUserId, RejectedByUserId)

alter table Lawyer
drop column ApprovalDate;

alter table Lawyer
drop column RejectionDate;

alter table Lawyer
drop column ApprovedByUserId;

alter table Lawyer
drop column RejectedByUserId;