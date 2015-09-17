alter table Lawyer
add ApprovedByUserId int null;

alter table Lawyer
add RejectedByUserId int null;

alter table Lawyer
add ApprovalDate datetime null;

alter table Lawyer
add RejectionDate datetime null;