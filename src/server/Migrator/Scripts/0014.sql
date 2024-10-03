DROP TABLE $schema$.[JiraProfileAccounts] 

GO

DROP TABLE $schema$.[JiraProfileProjects] 

GO

DROP TABLE $schema$.[JiraProfiles] 

GO

CREATE TABLE $schema$.[JiraProfileProjects] (
    [ProjectId] UNIQUEIDENTIFIER NOT NULL,
    [JiraProjectId] nvarchar(100) NOT NULL,
    [TempoToken] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_JiraProfileProjects] PRIMARY KEY ([ProjectId]),
    CONSTRAINT [FK_JiraProfileProjects_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES $schema$.[Projects] ([ProjectId]) ON DELETE CASCADE
);

GO

CREATE TABLE $schema$.[JiraProfileAccounts] (
    [ProjectId] UNIQUEIDENTIFIER NOT NULL,
    [CollaboratorId] UNIQUEIDENTIFIER NOT NULL,
    [CollaboratorRoleId] UNIQUEIDENTIFIER NOT NULL,
    [JiraAccountId] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_JiraProfileAccounts] PRIMARY KEY ([ProjectId], [CollaboratorId]),
	CONSTRAINT [FK_JiraProfileAccounts_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES dbo.[Projects] ([ProjectId]) ON DELETE CASCADE,
    CONSTRAINT [FK_JiraProfileAccounts_Collaborators_CollaboratorId] FOREIGN KEY ([CollaboratorId]) REFERENCES $schema$.[Collaborators] ([CollaboratorId]) ON DELETE CASCADE
);
