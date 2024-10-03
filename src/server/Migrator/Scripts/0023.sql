CREATE TABLE $schema$.[TaxPayments] (
    [TaxPaymentId] UNIQUEIDENTIFIER NOT NULL,
    [Total] decimal(19,4) NOT NULL, 
    [ITF] decimal(19,4) NOT NULL, 
    [Month] nvarchar(2) NOT NULL,
    [Year] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [PaidAt] date NULL,
    [DocumentUrl] nvarchar(500) NULL,
    [Status] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_TaxPayments] PRIMARY KEY ([TaxPaymentId])
);

GO

CREATE TABLE $schema$.[TaxPaymentItems] (
    [TaxPaymentId] UNIQUEIDENTIFIER NOT NULL,
    [TaxPaymentItemId] int NOT NULL,
    [Amount] decimal(19,4) NOT NULL, 
    [CollaboratorPaymentId] UNIQUEIDENTIFIER NULL,
    [InvoiceId] UNIQUEIDENTIFIER NULL,
    [PayrollPaymentId] UNIQUEIDENTIFIER NULL,
    [Type] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_TaxPaymentItems] PRIMARY KEY ([TaxPaymentId], [TaxPaymentItemId]),
	CONSTRAINT [FK_TaxPaymentItems_TaxPayments_TaxPaymentId] FOREIGN KEY ([TaxPaymentId]) REFERENCES dbo.[TaxPayments] ([TaxPaymentId]) ON DELETE CASCADE,
    CONSTRAINT [FK_TaxPaymentItems_CollaboratorPayments_CollaboratorPaymentId] FOREIGN KEY ([CollaboratorPaymentId]) REFERENCES $schema$.[CollaboratorPayments] ([CollaboratorPaymentId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TaxPaymentItems_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES $schema$.[Invoices] ([InvoiceId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TaxPaymentItems_PayrollPayments_CollaboratorPaymentId] FOREIGN KEY ([PayrollPaymentId]) REFERENCES $schema$.[PayrollPayments] ([PayrollPaymentId]) ON DELETE NO ACTION
);
