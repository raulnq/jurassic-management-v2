CREATE TABLE $schema$.[JiraProfiles] (
    [ClientId] UNIQUEIDENTIFIER NOT NULL,
    [TempoToken] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_JiraProfiles] PRIMARY KEY ([ClientId])
);

GO

CREATE TABLE $schema$.[JiraProfileProjects] (
    [ClientId] UNIQUEIDENTIFIER NOT NULL,
    [ProjectId] UNIQUEIDENTIFIER NOT NULL,
    [JiraProjectId] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_JiraProfileProjects] PRIMARY KEY ([ClientId], [ProjectId]),
    CONSTRAINT [FK_JiraProfileProjects_JiraProfiles_ClientId] FOREIGN KEY ([ClientId]) REFERENCES $schema$.[JiraProfiles] ([ClientId]) ON DELETE CASCADE,
    CONSTRAINT [FK_JiraProfileProjects_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES $schema$.[Projects] ([ProjectId]) ON DELETE CASCADE
);

GO

CREATE TABLE $schema$.[JiraProfileAccounts] (
    [ClientId] UNIQUEIDENTIFIER NOT NULL,
    [CollaboratorId] UNIQUEIDENTIFIER NOT NULL,
    [CollaboratorRoleId] UNIQUEIDENTIFIER NOT NULL,
    [JiraAccountId] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_JiraProfileAccounts] PRIMARY KEY ([ClientId], [CollaboratorId]),
    CONSTRAINT [FK_JiraProfileAccounts_JiraProfiles_ClientId] FOREIGN KEY ([ClientId]) REFERENCES $schema$.[JiraProfiles] ([ClientId]) ON DELETE CASCADE,
    CONSTRAINT [FK_JiraProfileAccounts_Collaborators_CollaboratorId] FOREIGN KEY ([CollaboratorId]) REFERENCES $schema$.[Collaborators] ([CollaboratorId]) ON DELETE CASCADE
);
