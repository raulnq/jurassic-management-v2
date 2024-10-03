CREATE TABLE $schema$.[CollaboratorRoles] (
    [CollaboratorRoleId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [FeeAmount] decimal(19,4) NOT NULL,
    [ProfitPercentage] decimal(19,4) NOT NULL,
    CONSTRAINT [PK_CollaboratorRoles] PRIMARY KEY ([CollaboratorRoleId])
);

GO