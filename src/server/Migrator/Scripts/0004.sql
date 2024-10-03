CREATE TABLE $schema$.[Projects] (
    [ClientId] UNIQUEIDENTIFIER NOT NULL,
    [ProjectId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Projects] PRIMARY KEY ([ProjectId]),
    CONSTRAINT [FK_Projects_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES $schema$.[Clients] ([ClientId]) ON DELETE CASCADE
);

GO