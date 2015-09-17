alter table Answer
add CreatedByUserId int null CONSTRAINT FK_Answer_CreatedByUserId FOREIGN KEY REFERENCES [User](UserId);

go;

update Answer
set CreatedByUserId = (select UserId from Lawyer where Lawyer.LawyerId = CreatedByLawyerId);

alter table Answer
alter column CreatedByUserId int not null;

alter table Answer
drop FK_Answer_CreatedByLawyerId;

alter table Answer
drop column CreatedByLawyerId;