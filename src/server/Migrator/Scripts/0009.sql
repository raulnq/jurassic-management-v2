CREATE TABLE $schema$.[ProformaDocuments] (
    [ProformaId] UNIQUEIDENTIFIER NOT NULL,
    [Url] nvarchar(500) NOT NULL,
    CONSTRAINT [PK_ProformaDocuments] PRIMARY KEY ([ProformaId]),
    CONSTRAINT [FK_ProformaDocuments_Proformas_ProformaId] FOREIGN KEY ([ProformaId]) REFERENCES $schema$.[Proformas] ([ProformaId]) ON DELETE CASCADE
);
