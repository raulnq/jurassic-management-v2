namespace WebAPI.Infrastructure.EntityFramework;

public static class Tables
{
    public static Table CollaboratorRoles = new Table("CollaboratorRoles");

    public static Table VwBankBalance = new Table("VwBankBalance");

    public static Table VwCollaboratorBalance = new Table("VwCollaboratorBalance");

    public static Table Transactions = new Table("Transactions");

    public static Table MoneyExchanges = new Table("MoneyExchanges");

    public static Table Collaborators = new Table("Collaborators");

    public static Table Periods = new Table("Periods");

    public static Table Projects = new Table("Projects");

    public static Table ClientContacts = new Table("ClientContacts");

    public static Table ProformaDocuments = new Table("ProformaDocuments");

    public static Table Clients = new Table("Clients");

    public static Table JiraProfileProjects = new Table("JiraProfileProjects");

    public static Table JiraProfiles = new Table("JiraProfiles");

    public static Table JiraProfileAccounts = new Table("JiraProfileAccounts");

    public static Table Proformas = new Table("Proformas");

    public static Table VwNotAddedToInvoiceProformas = new Table("VwNotAddedToInvoiceProformas");

    public static Table VwNotAddedToCollectionInvoices = new Table("VwNotAddedToCollectionInvoices");

    public static Table VwNotAddedToCollaboratorPaymentProformas = new Table("VwNotAddedToCollaboratorPaymentProformas");

    public static Table ProformaWeeks = new Table("ProformaWeeks");

    public static Table ProformaWeekWorkItems = new Table("ProformaWeekWorkItems");

    public static Table ProformaToInvoiceProcesses = new Table("ProformaToInvoiceProcesses");

    public static Table ProformaToInvoiceProcessItems = new Table("ProformaToInvoiceProcessItems");

    public static Table VwProformaToInvoiceProcessItems = new Table("VwProformaToInvoiceProcessItems");

    public static Table VwInvoiceToCollectionProcessItems = new Table("VwInvoiceToCollectionProcessItems");

    public static Table Invoices = new Table("Invoices");

    public static Table InvoiceToCollectionProcesses = new Table("InvoiceToCollectionProcesses");

    public static Table InvoiceToCollectionProcessItems = new Table("InvoiceToCollectionProcessItems");

    public static Table Collections = new Table("Collections");

    public static Table CollaboratorPayments = new Table("CollaboratorPayments");

    public static Table PayrollPayments = new Table("PayrollPayments");

    public static Table ProformaToCollaboratorPaymentProcesses = new Table("ProformaToCollaboratorPaymentProcesses");

    public static Table ProformaToCollaboratorPaymentProcessItems = new Table("ProformaToCollaboratorPaymentProcessItems");

    public static Table VwProformaToCollaboratorPaymentProcessItems = new Table("VwProformaToCollaboratorPaymentProcessItems");

    public static Table TaxPayments = new Table("TaxPayments");

    public static Table TaxPaymentItems = new Table("TaxPaymentItems");
}
