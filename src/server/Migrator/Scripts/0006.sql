CREATE TABLE $schema$.[ProformaToInvoiceProcesses] (
    [InvoiceId] UNIQUEIDENTIFIER NOT NULL,
    [ClientId] UNIQUEIDENTIFIER NOT NULL,
    [Currency] nvarchar(50) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_ProformaToInvoiceProcesses] PRIMARY KEY ([InvoiceId]),
);

GO

CREATE TABLE $schema$.[ProformaToInvoiceProcessItems] (
    [ProformaId] UNIQUEIDENTIFIER NOT NULL,
    [InvoiceId] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [PK_ProformaToInvoiceProcessItems] PRIMARY KEY ([ProformaId], [InvoiceId]),
    CONSTRAINT [FK_ProformaToInvoiceProcessItems_ProformaToInvoiceProcesses_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES $schema$.[ProformaToInvoiceProcesses] ([InvoiceId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProformaToInvoiceProcessItems_Proformas_ProformaId] FOREIGN KEY ([ProformaId]) REFERENCES $schema$.[Proformas] ([ProformaId]) ON DELETE CASCADE,
);

GO

CREATE TABLE $schema$.[Invoices] (
    [InvoiceId] UNIQUEIDENTIFIER NOT NULL,
    [ClientId] UNIQUEIDENTIFIER NOT NULL,
    [Number] nvarchar(50) NULL,
    [DocumentUrl] nvarchar(500) NULL,
    [Status] nvarchar(50) NOT NULL,
    [Total] decimal(19,4) NOT NULL, 
    [Taxes] decimal(19,4) NOT NULL,
    [SubTotal] decimal(19,4) NOT NULL, 
    [CreatedAt] datetimeoffset NOT NULL,
    [CanceledAt] datetimeoffset NULL,
    [IssuedAt] date NULL,
    [Currency] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY ([InvoiceId]),
    CONSTRAINT [FK_Invoices_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES $schema$.[Clients] ([ClientId]) ON DELETE CASCADE
);

GO

CREATE VIEW $schema$.[VwNotAddedToInvoiceProformas]
AS
  SELECT p.*
  FROM $schema$.[Proformas] p
  LEFT JOIN $schema$.ProformaToInvoiceProcessItems c on c.ProformaId = p.ProformaId
  LEFT JOIN $schema$.Invoices i on (i.InvoiceId = c.InvoiceId and i.Status!='Canceled')
  where i.InvoiceId is null

GO 

CREATE VIEW $schema$.VwProformaToInvoiceProcessItems
AS
  SELECT pi.InvoiceId, 
  pi.ProformaId, 
  p.Start as ProformaStart,
  p.[End] as ProformaEnd,
  p.Number as ProformaNumber,
  p.SubTotal as ProformaSubTotal,
  p.Commission as ProformaCommission,
  p.Total as ProformaTotal,
  p.Currency as ProformaCurrency,
  j.Name as ProjectName
  FROM $schema$.ProformaToInvoiceProcessItems pi
  JOIN $schema$.Proformas p on pi.ProformaId=p.ProformaId
  JOIN $schema$.Projects j on p.ProjectId=j.ProjectId