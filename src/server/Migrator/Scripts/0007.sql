CREATE TABLE $schema$.[InvoiceToCollectionProcesses] (
    [CollectionId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ClientId] UNIQUEIDENTIFIER NOT NULL,
    [Currency] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_InvoiceToCollectionProcesses] PRIMARY KEY ([CollectionId]),
);

GO

CREATE TABLE $schema$.[InvoiceToCollectionProcessItems] (
    [CollectionId] UNIQUEIDENTIFIER NOT NULL,
    [InvoiceId] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [PK_InvoiceToCollectionProcessItems] PRIMARY KEY ([CollectionId], [InvoiceId]),
    CONSTRAINT [FK_InvoiceToCollectionProcessItems_InvoiceToCollectionProcesses_CollectionId] FOREIGN KEY ([CollectionId]) REFERENCES $schema$.[InvoiceToCollectionProcesses] ([CollectionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_InvoiceToCollectionProcessItems_Invoices_Invoice] FOREIGN KEY ([InvoiceId]) REFERENCES $schema$.[Invoices] ([InvoiceId]) ON DELETE CASCADE,
);

GO

CREATE TABLE $schema$.[Collections] (
    [CollectionId] UNIQUEIDENTIFIER NOT NULL,
    [ClientId] UNIQUEIDENTIFIER NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [Total] decimal(19,4) NOT NULL, 
    [Commission] decimal(19,4) NOT NULL, 
    [Number] nvarchar(50) NULL,
    [ITF] decimal(19,4) NOT NULL, 
    [CreatedAt] datetimeoffset NOT NULL,
    [CanceledAt] datetimeoffset NULL,
    [ConfirmedAt] date NULL,
    [Currency] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Collections] PRIMARY KEY ([CollectionId]),
);

GO

CREATE VIEW $schema$.[VwNotAddedToCollectionInvoices]
AS
  SELECT p.*
  FROM $schema$.[Invoices] p
  LEFT JOIN $schema$.InvoiceToCollectionProcessItems c on c.InvoiceId = p.InvoiceId
  LEFT JOIN $schema$.Collections i on (i.CollectionId = c.CollectionId and i.Status!='Canceled')
  where i.CollectionId is null

GO

CREATE VIEW $schema$.VwInvoiceToCollectionProcessItems
AS
  SELECT 
  pi.CollectionId,
  i.InvoiceId,
  i.Currency as InvoiceCurreny,
  i.SubTotal as InvoiceSubTotal,
  i.Taxes as InvoiceTaxes,
  i.Total as InvoiceTotal,
  i.Number as InvoiceNumber
  FROM $schema$.InvoiceToCollectionProcessItems pi
  JOIN $schema$.Invoices i on i.InvoiceId=pi.InvoiceId