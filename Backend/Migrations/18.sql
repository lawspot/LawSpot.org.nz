alter table Answer
alter column CreatedOn datetimeoffset not null;

alter table Answer
alter column ApprovalDate datetimeoffset null;

alter table Answer
alter column RejectionDate datetimeoffset null;



alter table Lawyer
alter column CreatedOn datetimeoffset not null;

alter table Lawyer
alter column ApprovalDate datetimeoffset null;

alter table Lawyer
alter column RejectionDate datetimeoffset null;



alter table Migrations
alter column RunAt datetimeoffset not null;



alter table Question
alter column CreatedOn datetimeoffset not null;

alter table Question
alter column ApprovalDate datetimeoffset null;

alter table Question
alter column RejectionDate datetimeoffset null;



alter table [User]
alter column CreatedOn datetimeoffset not null;