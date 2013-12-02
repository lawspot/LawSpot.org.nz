/****** Object:  Table [dbo].[Lawyer]    Script Date: 2/12/2013 9:15:03 p.m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [dbo].[User] ADD
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[YearOfAdmission] [int] NULL,
	[SpecialisationCategoryId] [int] NULL,
	[EmployerName] [nvarchar](100) NULL,
	[ApprovedAsLawyer] [bit] NULL,
	[ReviewDate] [datetimeoffset](7) NULL,
	[ReviewedByUserId] [int] NULL,
	[RejectionReason] [nvarchar](max) NULL

GO

ALTER TABLE [dbo].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_Category] FOREIGN KEY([SpecialisationCategoryId])
REFERENCES [dbo].[Category] ([CategoryId])
GO

ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Category]
GO

UPDATE u
SET u.[FirstName] = l.[FirstName],
	u.[LastName] = l.[LastName],
	u.[YearOfAdmission] = l.[YearOfAdmission],
	u.[SpecialisationCategoryId] = l.[SpecialisationCategoryId],
	u.[EmployerName] = l.[EmployerName],
	u.[ApprovedAsLawyer] = l.[Approved],
	u.[ReviewDate] = l.[ReviewDate],
	u.[ReviewedByUserId] = l.[ReviewedByUserId],
	u.[RejectionReason] = l.[RejectionReason]
FROM [dbo].[User] u
JOIN [dbo].[Lawyer] l ON l.UserId = u.UserId
GO

DROP TABLE [dbo].[Lawyer]