alter table [User]
add CanAnswerQuestions bit null;

alter table [User]
add CanVetQuestions bit null;

alter table [User]
add CanVetAnswers bit null;

alter table [User]
add CanVetLawyers bit null;

go;

update [User]
set CanAnswerQuestions = (case when (select count(*) from Lawyer where UserId = [User].UserId) > 0 then 1 else 0 end),
CanVetQuestions = IsVolunteerAdmin,
CanVetAnswers = IsCLCLawyer,
CanVetLawyers = IsVolunteerAdmin
from [User]

alter table [User]
alter column CanAnswerQuestions bit not null;

alter table [User]
alter column CanVetQuestions bit not null;

alter table [User]
alter column CanVetAnswers bit not null;

alter table [User]
alter column CanVetLawyers bit not null;

alter table [User]
drop column IsVolunteerAdmin;

alter table [User]
drop column IsCLCLawyer;