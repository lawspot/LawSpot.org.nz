alter table [User] add IsVolunteerAdmin bit null
alter table [User] add IsLawyer bit null
alter table [User] add IsCLCLawyer bit null
go

update [User] set IsVolunteerAdmin = 0, IsLawyer = 0, IsCLCLawyer = 0

alter table [User] alter column IsVolunteerAdmin bit not null
alter table [User] alter column IsLawyer bit not null
alter table [User] alter column IsCLCLawyer bit not null