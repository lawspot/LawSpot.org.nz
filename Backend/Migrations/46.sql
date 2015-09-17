ALTER TABLE Answer
ADD RecommendApproval BIT NULL
GO

UPDATE Answer
SET RecommendApproval = 0
GO

ALTER TABLE Answer
ALTER COLUMN RecommendApproval BIT NOT NULL