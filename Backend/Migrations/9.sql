alter table Question
add Slug varchar(70) not null;
go

CREATE UNIQUE NONCLUSTERED INDEX IX_Question_Slug ON dbo.Question (Slug);