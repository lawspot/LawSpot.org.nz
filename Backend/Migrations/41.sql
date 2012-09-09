ALTER TABLE Question
ADD CONSTRAINT [FK_Question_User_2]
FOREIGN KEY ([ReviewedByUserId])
REFERENCES [dbo].[User] ([UserId])