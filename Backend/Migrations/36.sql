delete Category where Name = 'Tax Law'
delete Category where Name = 'Building Law'
insert Category (Name, Slug, ApprovedQuestionCount) values ('Maori Legal Issues', 'maori-legal', 0)
insert Category (Name, Slug, ApprovedQuestionCount) values ('Youth Law', 'youth-law', 0)
insert Category (Name, Slug, ApprovedQuestionCount) values ('Education', 'education', 0)
insert Category (Name, Slug, ApprovedQuestionCount) values ('Driving and Traffic', 'driving-traffic', 0)
update Category set Name = 'Health, Disability and ACC' where Name = 'Health and ACC'