ALTER TABLE Category
ADD ApprovedQuestionCount INT NULL
GO

UPDATE Category
SET ApprovedQuestionCount = (SELECT COUNT(*) FROM Question WHERE Question.CategoryId = Category.CategoryId AND Approved = 1)

ALTER TABLE Category
ALTER COLUMN ApprovedQuestionCount INT NOT NULL