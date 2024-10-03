ALTER TABLE $schema$.[Collaborators] ADD [Email] nvarchar(255) NULL;

GO

ALTER TABLE $schema$.[Proformas] ADD [Note] nvarchar(1000) NULL;

GO

ALTER VIEW $schema$.[VwProformaToCollaboratorPaymentProcessItems]
AS
  SELECT 
  pi.CollaboratorPaymentId, 
  pi.ProformaId, 
  pi.Week, 
  pi.CollaboratorId, 
  c.Name as CollaboratorName,
  c.Email as CollaboratorEmail,
  j.Name as ProjectName, 
  p.Number as ProformaNumber, 
  p.Currency as ProformaCurrency,
  p.Note as ProformaNote,
  pw.Start as ProformaWeekStart,
  pw.[End] as ProformaWeekEnd,
  wi.Hours,
  wi.FreeHours,
  wi.FeeAmount,
  wi.SubTotal,
  wi.ProfitPercentage,
  wi.ProfitAmount
  FROM [dbo].ProformaToCollaboratorPaymentProcessItems pi
  JOIN [dbo].ProformaWeekWorkItems wi on pi.ProformaId=wi.ProformaId and pi.CollaboratorId=wi.CollaboratorId and pi.Week=wi.Week
  JOIN [dbo].ProformaWeeks pw on pw.ProformaId=wi.ProformaId and pw.Week = wi.Week
  JOIN [dbo].Proformas p on pw.ProformaId=p.ProformaId
  JOIN [dbo].Projects j on p.ProjectId=j.ProjectId
  JOIN [dbo].Collaborators c on c.CollaboratorId=pi.CollaboratorId
GO


