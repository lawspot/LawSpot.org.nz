alter table [User]
add ResetPasswordToken varchar(50) null

alter table [User]
add ResetPasswordTokenExpiry datetimeoffset null