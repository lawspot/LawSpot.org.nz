alter table [User]
add EmailValidated bit null;

alter table [User]
add EmailValidationToken varchar(50) null;

go;

update [User]
set EmailValidated = 1;

alter table [User]
alter column EmailValidated bit not null;