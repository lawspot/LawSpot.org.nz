ALTER TABLE Category
ADD AnsweredQuestionCount INT NULL
GO

UPDATE Category
SET AnsweredQuestionCount = (
	SELECT COUNT(*) FROM Question
	WHERE Question.CategoryId = Category.CategoryId
	AND Question.Approved = 1
	AND EXISTS (SELECT * FROM Answer WHERE Question.QuestionId = Answer.QuestionId AND Answer.Approved = 1)
)

ALTER TABLE Category
ALTER COLUMN AnsweredQuestionCount INT NOT NULL