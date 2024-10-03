ALTER TABLE $schema$.JiraProfileProjects
DROP CONSTRAINT PK_JiraProfileProjects

GO

ALTER TABLE $schema$.JiraProfileProjects
ADD CONSTRAINT PK_JiraProfileProjects PRIMARY KEY (ProjectId,JiraProjectId)