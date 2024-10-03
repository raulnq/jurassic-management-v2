CREATE TABLE $schema$.[Clients] (
    [ClientId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [PhoneNumber] nvarchar(50) NOT NULL,
    [DocumentNumber] nvarchar(50) NOT NULL,
    [Address] nvarchar(500) NOT NULL,
    [TaxesExpensesPercentage] decimal(19,4) NOT NULL,
    [AdministrativeExpensesPercentage] decimal(19,4) NOT NULL,
    [BankingExpensesPercentage] decimal(19,4) NOT NULL,
    [MinimumBankingExpenses] decimal(19,4) NOT NULL,
    [PenaltyMinimumHours] decimal(19,4) NOT NULL,
    [PenaltyAmount] decimal(19,4) NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY ([ClientId])
);

GO