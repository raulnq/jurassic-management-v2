CREATE TABLE $schema$.[ProformaToCollaboratorPaymentProcesses] (
    [CollaboratorPaymentId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CollaboratorId] UNIQUEIDENTIFIER NOT NULL, 
    [Currency] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_ProformaToCollaboratorPaymentProcesses] PRIMARY KEY ([CollaboratorPaymentId]),
);

GO

CREATE TABLE $schema$.[ProformaToCollaboratorPaymentProcessItems] (
    [CollaboratorPaymentId] UNIQUEIDENTIFIER NOT NULL,
    [ProformaId] UNIQUEIDENTIFIER NOT NULL, 
    [Week] int NOT NULL, 
    [CollaboratorId] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [PK_ProformaToCollaboratorPaymentProcessItems] PRIMARY KEY ([CollaboratorPaymentId], [ProformaId], [Week], [CollaboratorId]),
    CONSTRAINT [FK_ProformaToCollaboratorPaymentProcessItems_ProformaToCollaboratorPaymentProcesses_CollaboratorPaymentId] FOREIGN KEY ([CollaboratorPaymentId]) REFERENCES $schema$.[ProformaToCollaboratorPaymentProcesses] ([CollaboratorPaymentId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProformaToCollaboratorPaymentProcessItems_ProformaWeekWorkItem] FOREIGN KEY ([ProformaId],[Week],[CollaboratorId]) REFERENCES $schema$.[ProformaWeekWorkItems] ([ProformaId],[Week],[CollaboratorId]) ON DELETE CASCADE,
);

GO

CREATE TABLE $schema$.[CollaboratorPayments] (
    [CollaboratorPaymentId] UNIQUEIDENTIFIER NOT NULL,
    [CollaboratorId] UNIQUEIDENTIFIER NOT NULL, 
    [Number] nvarchar(50) NULL,
    [Status] nvarchar(50) NOT NULL,
    [GrossSalary] decimal(19,4) NOT NULL, 
    [NetSalary] decimal(19,4) NOT NULL, 
    [ITF] decimal(19,4) NOT NULL, 
    [Withholding] decimal(19,4) NOT NULL, 
    [CreatedAt] datetimeoffset NOT NULL,
    [PaidAt] date NULL,
    [CanceledAt] datetimeoffset NULL,
    [DocumentUrl] nvarchar(500) NULL,
    [ConfirmedAt] date NULL,
    [Currency] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_CollaboratorPayments] PRIMARY KEY ([CollaboratorPaymentId]),
    CONSTRAINT [FK_CollaboratorPayments_Collaborators_CollaboratorId] FOREIGN KEY ([CollaboratorId]) REFERENCES $schema$.[Collaborators] ([CollaboratorId]) ON DELETE CASCADE,
);

GO

CREATE VIEW $schema$.[VwNotAddedToCollaboratorPaymentProformas]
AS
  SELECT p.Status, p.Currency, p.Number, p.ProjectId, w.Start, w.[End], pi.*
  FROM $schema$.[ProformaWeekWorkItems] pi
  JOIN $schema$.[ProformaWeeks] w on pi.ProformaId=w.ProformaId and pi.Week=w.Week
  JOIN $schema$.[Proformas] p on p.ProformaId=pi.ProformaId
  LEFT JOIN $schema$.ProformaToCollaboratorPaymentProcessItems c on c.ProformaId = pi.ProformaId and c.Week=pi.Week and c.CollaboratorId=pi.CollaboratorId
  LEFT JOIN $schema$.CollaboratorPayments i on (i.CollaboratorId = c.CollaboratorId and i.Status!='Canceled')
  where i.CollaboratorPaymentId is null

  GO

CREATE VIEW $schema$.VwProformaToCollaboratorPaymentProcessItems
AS
  SELECT 
  pi.CollaboratorPaymentId, 
  pi.ProformaId, 
  pi.Week, 
  pi.CollaboratorId, 
  j.Name as ProjectName, 
  p.Number as ProformaNumber, 
  p.Currency as ProformaCurrency,
  pw.Start as ProformaWeekStart,
  pw.[End] as ProformaWeekEnd,
  wi.Hours,
  wi.FreeHours,
  wi.FeeAmount,
  wi.SubTotal,
  wi.ProfitPercentage,
  wi.ProfitAmount
  FROM $schema$.ProformaToCollaboratorPaymentProcessItems pi
  JOIN $schema$.ProformaWeekWorkItems wi on pi.ProformaId=wi.ProformaId and pi.CollaboratorId=wi.CollaboratorId and pi.Week=wi.Week
  JOIN $schema$.ProformaWeeks pw on pw.ProformaId=wi.ProformaId and pw.Week = wi.Week
  JOIN $schema$.Proformas p on pw.ProformaId=p.ProformaId
  JOIN $schema$.Projects j on p.ProjectId=j.ProjectId