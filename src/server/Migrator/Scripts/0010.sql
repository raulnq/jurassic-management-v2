CREATE TABLE $schema$.[Transactions] (
    [TransactionId] UNIQUEIDENTIFIER NOT NULL,
    [Description] nvarchar(1000) NOT NULL,
    [Number] nvarchar(50) NULL,
    [DocumentUrl] nvarchar(500) NULL,
    [Currency] nvarchar(50) NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [SubTotal] decimal(19,4) NOT NULL,
    [Taxes] decimal(19,4) NOT NULL,
    [Total] decimal(19,4) NOT NULL,
    [ITF] decimal(19,4) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [IssuedAt] date NOT NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY ([TransactionId])
);

GO