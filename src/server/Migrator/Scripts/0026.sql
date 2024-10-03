ALTER TABLE $schema$.[Invoices] ADD [TaxPaymentId] UNIQUEIDENTIFIER NULL

GO

ALTER TABLE $schema$.[PayrollPayments] ADD [TaxPaymentId] UNIQUEIDENTIFIER NULL

GO

ALTER TABLE $schema$.[CollaboratorPayments] ADD [TaxPaymentId] UNIQUEIDENTIFIER NULL

GO

ALTER TABLE $schema$.[PayrollPayments] ADD [ExcludeFromTaxes] bit NOT NULL CONSTRAINT PayrollPayments_ExcludeFromTaxes_Default default 0;