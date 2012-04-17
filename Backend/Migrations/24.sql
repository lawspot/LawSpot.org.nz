alter table Lawyer
add RejectionReason nvarchar(1000) null;

alter table Question
add RejectionReason nvarchar(1000) null;

alter table Answer
add RejectionReason nvarchar(1000) null;