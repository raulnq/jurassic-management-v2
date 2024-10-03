CREATE TABLE $schema$.[Collaborators] (
    [CollaboratorId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [WithholdingPercentage] decimal(19,4) NOT NULL,
    CONSTRAINT [PK_Collaborators] PRIMARY KEY ([CollaboratorId])
);

GO