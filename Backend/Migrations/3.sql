alter table Category
add Slug varchar(30) null;
go

update Category set Slug = 'auto-accidents' where CategoryId = 1;
update Category set Slug = 'bankruptcy' where CategoryId = 2;
update Category set Slug = 'business' where CategoryId = 3;
update Category set Slug = 'collections-and-debt' where CategoryId = 4;
update Category set Slug = 'consumer-and-lemon' where CategoryId = 5;
update Category set Slug = 'criminal-defence' where CategoryId = 6;
update Category set Slug = 'dui-dwi' where CategoryId = 7;
update Category set Slug = 'divorce-marriage-alimony' where CategoryId = 8;
update Category set Slug = 'immigration-naturalization' where CategoryId = 9;
update Category set Slug = 'insurance-law' where CategoryId = 10;
update Category set Slug = 'landlord-tenant' where CategoryId = 11;
update Category set Slug = 'medical-malpractice' where CategoryId = 12;
update Category set Slug = 'personal-injury' where CategoryId = 13;
update Category set Slug = 'real-estate' where CategoryId = 14;
update Category set Slug = 'traffic' where CategoryId = 15;
update Category set Slug = 'wills-trusts-probate' where CategoryId = 16;

alter table Category
alter column Slug varchar(30) not null;

CREATE UNIQUE NONCLUSTERED INDEX IX_Category_Slug ON dbo.Category (Slug);