ALTER TABLE Answer
ADD OriginalDetails nvarchar(max) null

ALTER TABLE Question
ADD OriginalTitle nvarchar(150) null

ALTER TABLE Question
ADD OriginalDetails nvarchar(600) null