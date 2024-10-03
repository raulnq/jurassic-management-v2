CREATE TABLE $schema$.[ClientContacts] (
    [ClientId] UNIQUEIDENTIFIER NOT NULL,
    [ClientContactId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    CONSTRAINT [PK_ClientContacts] PRIMARY KEY ([ClientContactId]),
    CONSTRAINT [FK_ClientContacts_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES $schema$.[Clients] ([ClientId]) ON DELETE CASCADE
);

GO