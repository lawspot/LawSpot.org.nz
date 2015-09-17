ALTER TABLE Answer
ADD CONSTRAINT [FK_Answer_ReviewedByUserId]
FOREIGN KEY ([ReviewedByUserId])
REFERENCES [dbo].[User] ([UserId])