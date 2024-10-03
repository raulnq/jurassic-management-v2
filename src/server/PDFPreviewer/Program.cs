// See https://aka.ms/new-console-template for more information
using QuestPDF.Previewer;
using WebAPI.ProformaDocuments;
using WebAPI.Proformas;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var getProformaResult = new GetProforma.Result()
{
    Number = "20250111-1",
    ProjectName = "Makro B2B",
    Start = new DateTime(2025, 01, 05),
    End = new DateTime(2025, 01, 11),
    SubTotal = 1000,
    Commission = 100,
    Discount = 10,
    Total = 1090,
    ClientAddress = "Av. 4to anillo y Paraguá. Santa Cruz, Bolivia",
    ClientDocumentNumber = "351418020",
    ClientName = "Comercializadora Retail Lider S.A.",
    ClientPhoneNumber = "348-9999"
};

var collaboratorId = Guid.NewGuid();

var listProformaWeekWorkItemsResult = new ListProformaWeekWorkItems.Result[]
{
    new ListProformaWeekWorkItems.Result(){
        SubTotal = 100,
        FeeAmount = 10,
        Hours = 10,
        CollaboratorRoleId = Guid.NewGuid(),
        CollaboratorRoleName = "DeveloperA"
    },
    new ListProformaWeekWorkItems.Result(){
        SubTotal = 200,
        FeeAmount = 10,
        Hours = 20,
        CollaboratorRoleId = Guid.NewGuid(),
        CollaboratorRoleName = "DeveloperB"
    },
    new ListProformaWeekWorkItems.Result(){
        SubTotal = 500,
        FeeAmount = 25,
        Hours = 20,
        CollaboratorRoleId = collaboratorId,
        CollaboratorRoleName = "DeveloperC"
    },
    new ListProformaWeekWorkItems.Result(){
        SubTotal = 200,
        FeeAmount = 25,
        Hours = 8,
        CollaboratorRoleId = collaboratorId,
        CollaboratorRoleName = "DeveloperC"
    }
};

var document = new ProformaPdf(getProformaResult, listProformaWeekWorkItemsResult);

document.ShowInPreviewer();