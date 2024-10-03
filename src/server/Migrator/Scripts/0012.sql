CREATE VIEW $schema$.VwCollaboratorBalance
AS
SELECT cp.CollaboratorId, c.Name, cp.GrossSalary, cp.Withholding, cp.NetSalary, cp.Number, cp.Currency, cp.PaidAt as Date, null as Start, null as [End], -1 as Sign
	FROM $schema$.[CollaboratorPayments] cp
	JOIN $schema$.[Collaborators] c on c.CollaboratorId=cp.CollaboratorId
	WHERE Status in ('Paid', 'Confirmed')
UNION ALL
SELECT wi.CollaboratorId, c.Name, wi.GrossSalary, wi.Withholding, wi.NetSalary, p.Number, p.Currency, CONVERT(DATE, p.IssuedAt) as Date, w.Start, w.[End], 1 as Sign
	FROM $schema$.ProformaWeekWorkItems wi
	JOIN $schema$.[Collaborators] c on c.CollaboratorId=wi.CollaboratorId
	JOIN $schema$.ProformaWeeks w on wi.ProformaId=w.ProformaId and wi.Week=w.Week
	JOIN $schema$.Proformas p on p.ProformaId=w.ProformaId
	WHERE Status = 'Issued'