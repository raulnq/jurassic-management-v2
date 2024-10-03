CREATE VIEW $schema$.VwBankBalance
AS
SELECT TransactionId as RecordId, IssuedAt, Type, Description, Currency, SubTotal, Taxes, Total, ITF, Number, 'Transactions' as Source, CreatedAt, CASE WHEN Type='Expenses' THEN -1 ELSE 1 END as Sign
FROM $schema$.[Transactions]
UNION ALL
SELECT cp.CollaboratorId, PaidAt, 'Expenses', CONCAT('Payment to collaborator ',c.Name) , Currency, NetSalary, 0, NetSalary, ITF, Number, 'CollaboratorPayments', CreatedAt, -1
FROM $schema$.[CollaboratorPayments] cp
JOIN $schema$.[Collaborators] c on c.CollaboratorId=cp.CollaboratorId
WHERE Status in ('Paid', 'Confirmed')
UNION ALL
SELECT c.CollectionId, ConfirmedAt, 'Incomes',  CONCAT('Payment from invoices ', STRING_AGG(i.Number, ', ')), c.Currency, c.Total, 0, c.Total, ITF, c.Number, 'Collections', c.CreatedAt, 1
FROM $schema$.[Collections] c
JOIN $schema$.[InvoiceToCollectionProcessItems] ic on c.CollectionId=ic.CollectionId
JOIN $schema$.[Invoices] i on i.InvoiceId=ic.InvoiceId
WHERE c.Status='Confirmed'
group by c.ClientId, c.CollectionId, c.ConfirmedAt, c.CreatedAt, c.Currency, c.ITF, c.Status, c.Total, c.Commission,  c.Number
UNION ALL
SELECT c.CollectionId, ConfirmedAt, 'Expenses', CONCAT('Bank commission from invoices ', STRING_AGG(i.Number, ', ')), c.Currency, c.Commission, 0, c.Commission, 0, c.Number, 'Collections', c.CreatedAt, -1
FROM $schema$.[Collections] c
JOIN $schema$.[InvoiceToCollectionProcessItems] ic on c.CollectionId=ic.CollectionId
JOIN $schema$.[Invoices] i on i.InvoiceId=ic.InvoiceId
WHERE c.Status='Confirmed' and c.Commission>0
group by c.ClientId, c.CollectionId, c.ConfirmedAt, c.CreatedAt, c.Currency, c.ITF, c.Status, c.Total, c.Commission, c.Number