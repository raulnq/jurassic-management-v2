CREATE TABLE $schema$.[Proformas] (
    [ProformaId] UNIQUEIDENTIFIER NOT NULL,
    [Start] date NOT NULL,
    [End] date NOT NULL,
    [Number] nvarchar(50) NOT NULL,
    [ProjectId] UNIQUEIDENTIFIER NOT NULL,
    [Total] decimal(19,4) NOT NULL,
    [SubTotal] decimal(19,4) NOT NULL,
    [Commission] decimal(19,4) NOT NULL,
    [Discount] decimal(19,4) NOT NULL,
    [MinimumHours] decimal(19,4) NOT NULL,
    [PenaltyMinimumHours] decimal(19,4) NOT NULL,
    [TaxesExpensesPercentage] decimal(19,4) NOT NULL,
    [AdministrativeExpensesPercentage] decimal(19,4) NOT NULL,
    [BankingExpensesPercentage] decimal(19,4) NOT NULL,
    [MinimumBankingExpenses] decimal(19,4) NOT NULL,
    [TaxesExpensesAmount] decimal(19,4) NOT NULL,
    [AdministrativeExpensesAmount] decimal(19,4) NOT NULL,
    [BankingExpensesAmount] decimal(19,4) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [IssuedAt] date NULL,
    [CanceledAt] datetimeoffset NULL,
    [Status] nvarchar(50) NOT NULL,
    [Currency] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Proformas] PRIMARY KEY ([ProformaId]),
    CONSTRAINT [FK_Proformas_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES $schema$.[Projects] ([ProjectId]) ON DELETE CASCADE
);

GO

CREATE TABLE $schema$.[ProformaWeeks] (
    [ProformaId] UNIQUEIDENTIFIER NOT NULL,
    [Week] int NOT NULL,
    [Start] date NOT NULL,
    [End] date NOT NULL,
    [Penalty] decimal(19,4) NOT NULL,
    [SubTotal] decimal(19,4) NOT NULL,   
    CONSTRAINT [PK_ProformaWeeks] PRIMARY KEY ([ProformaId], [Week]),
    CONSTRAINT [FK_ProformaWeeks_Proformas_ProformaId] FOREIGN KEY ([ProformaId]) REFERENCES $schema$.[Proformas] ([ProformaId]) ON DELETE CASCADE
);

GO

CREATE TABLE $schema$.[ProformaWeekWorkItems] (
    [ProformaId] UNIQUEIDENTIFIER NOT NULL,
    [Week] int NOT NULL,
    [CollaboratorId] UNIQUEIDENTIFIER NOT NULL,
    [CollaboratorRoleId] UNIQUEIDENTIFIER NOT NULL,
    [Hours] decimal(19,4) NOT NULL,
    [FreeHours] decimal(19,4) NOT NULL,
    [FeeAmount] decimal(19,4) NOT NULL,
    [SubTotal] decimal(19,4) NOT NULL,
    [ProfitAmount] decimal(19,4) NOT NULL,
    [ProfitPercentage] decimal(19,4) NOT NULL,
    [Withholding] decimal(19,4) NOT NULL,
    [WithholdingPercentage] decimal(19,4) NOT NULL,
    [NetSalary] decimal(19,4) NOT NULL,
    [GrossSalary] decimal(19,4) NOT NULL,
    CONSTRAINT [PK_ProformaWeekWorkItems] PRIMARY KEY ([ProformaId], [Week], [CollaboratorId]),
    CONSTRAINT [FK_ProformaWeekWorkItems_ProformaWeeks] FOREIGN KEY ([ProformaId], [Week]) REFERENCES $schema$.[ProformaWeeks] ([ProformaId], [Week]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProformaWeekWorkItems_Collaborators_CollaboratorId] FOREIGN KEY ([CollaboratorId]) REFERENCES $schema$.[Collaborators] ([CollaboratorId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProformaWeekWorkItems_Proformas_CollaboratorRoleId] FOREIGN KEY ([CollaboratorRoleId]) REFERENCES $schema$.[CollaboratorRoles] ([CollaboratorRoleId]) ON DELETE CASCADE
);

GO