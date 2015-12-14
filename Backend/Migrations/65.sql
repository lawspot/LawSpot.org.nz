CREATE TABLE Settings (
	SettingsId INT NOT NULL IDENTITY CONSTRAINT PK_Settings PRIMARY KEY,
	[Key] NVARCHAR(50) NOT NULL,
	[Value] NVARCHAR(MAX) NOT NULL
)
GO

CREATE UNIQUE NONCLUSTERED INDEX UK_Settings_Key ON dbo.[Settings] ([Key]);
GO

INSERT Settings ([Key], Value) VALUES ('AllowQuestions', 'true')
INSERT Settings ([Key], Value) VALUES ('CannotAskQuestionsMessage', 'Thank you for visiting LawSpot. Our volunteers are away over the holidays.  You may want to ask your local Community Law Centre for help in the meantime. You can find the contact details for your local Centre by clicking <a href="http://www.communitylaw.org.nz/your-local-centre/find-a-community-law-centre/">here</a>.')