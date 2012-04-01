ALTER TABLE dbo.Answer
	DROP CONSTRAINT FK_Answer_User;
alter table Answer drop column CreatedByUserId
go

alter table Answer add CreatedByLawyerId int not null
CONSTRAINT FK_Answer_CreatedByLawyerId FOREIGN KEY REFERENCES Lawyer(LawyerId)