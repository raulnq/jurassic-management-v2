using Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebAPI.Proformas;

namespace WebAPI.ProformaDocuments
{
    public class ProformaPdf : IDocument
    {
        public GetProforma.Result GetProforma { get; }

        public IEnumerable<ListProformaWeekWorkItems.Result> ListProformaWeekWorkItems { get; }

        public ProformaPdf(GetProforma.Result getProforma,
            IEnumerable<ListProformaWeekWorkItems.Result> listProformaWeekWorkItems)
        {
            GetProforma = getProforma;
            ListProformaWeekWorkItems = listProformaWeekWorkItems;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.DefaultTextStyle(x => x.FontSize(10)).Row(row =>
            {
                row.ConstantItem(125).Height(135).Image("logo.jpg");
                row.RelativeItem().Column(column =>
                {
                    column.Item().PaddingTop(20).Text("Jurassic Technologies S.A.C").AlignCenter().FontSize(15).Bold();
                    column.Item().PaddingTop(20).Text("Av. Los Alarifes 1121").AlignCenter();
                    column.Item().Text("Block I Dpto 607").AlignCenter();
                    column.Item().Text("Cond. Floresta Sur").AlignCenter();
                    column.Item().Text("Chorrillos - Lima").AlignCenter();
                    column.Item().Text("Lima").AlignCenter();
                });
                row.ConstantItem(125).Column(column =>
                {
                    column.Item().PaddingTop(60).PaddingBottom(10).Text($"Proforma {GetProforma.Number}").AlignCenter().Bold();
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Proyecto:").Bold();
                        row.RelativeItem().Text(GetProforma.ProjectName).AlignRight();
                    });
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Desde:").Bold();
                        row.RelativeItem().Text(GetProforma.Start.ToListFormat()).AlignRight();
                    });
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Hasta:").Bold();
                        row.RelativeItem().Text(GetProforma.End.ToListFormat()).AlignRight();
                    });
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.DefaultTextStyle(x => x.FontSize(10)).PaddingTop(25).Column(column =>
            {
                column.Spacing(20);

                column.Item().Element(ComposeClient);

                column.Item().Element(ComposeTable);

                column.Item().Element(ComposeTerms);
            });
        }

        void ComposeClient(IContainer container)
        {
            container.DefaultTextStyle(x => x.FontSize(10)).Row(row =>
            {
                row.ConstantItem(125).Column(column =>
                {
                    column.Spacing(5);
                    column.Item().Text("Client:");
                    column.Item().Text("Dirección:");
                    column.Item().Text("NIT:");
                    column.Item().Text("Telefono:");
                });

                row.RelativeItem().Column(column =>
                {
                    column.Spacing(5);
                    column.Item().Text(GetProforma.ClientName);
                    column.Item().Text(GetProforma.ClientAddress);
                    column.Item().Text(GetProforma.ClientDocumentNumber);
                    column.Item().Text(GetProforma.ClientPhoneNumber);
                });
            });
        }

        void ComposeTerms(IContainer container)
        {
            container.DefaultTextStyle(x => x.FontSize(10)).Column(column =>
            {
                //column.Spacing(5);
                column.Item().Text("Términos y Condiciones").Bold();
                column.Item().Text("1) Todos los costos están expresados en Dólares Americanos (USD).");
                column.Item().Text("2) El precio representa una hora de trabajo (menos para Comisión Bancaria).");
                column.Item().Text("3) Los items resaltados en color, representan servicios cobrados de manera independiente y no se miden en horas.");
            });
        }

        void ComposeTable(IContainer container)
        {
            container.DefaultTextStyle(x => x.FontSize(10)).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Padding(2).PaddingRight(5).Text("Recurso");
                    header.Cell().Element(CellStyle).Padding(2).PaddingRight(5).AlignRight().Text("Precio");
                    header.Cell().Element(CellStyle).Padding(2).PaddingRight(5).AlignRight().Text("Horas");
                    header.Cell().Element(CellStyle).Padding(2).PaddingRight(5).AlignRight().Text("SubTotal");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.Bold()).Border(1).BorderColor(Colors.Black);
                    }
                });

                foreach (var item in ListProformaWeekWorkItems.GroupBy(wi => new { wi.CollaboratorRoleName }))
                {

                    var subtotal = item.Sum(i => i.SubTotal);
                    var feeAmount = item.Average(i => i.FeeAmount);
                    var hours = item.Sum(i => i.Hours);
                    table.Cell().Element(CellStyle).Padding(2).PaddingRight(5).Text(item.Key.CollaboratorRoleName);
                    table.Cell().Element(CellStyle).Padding(2).PaddingRight(5).AlignRight().Row(r =>
                    {
                        r.RelativeItem().Text("$");
                        r.RelativeItem(3).Text($"{feeAmount:F2}").AlignRight();
                    });
                    table.Cell().Element(CellStyle).Padding(2).PaddingRight(5).AlignRight().Text($"{hours:F2}");
                    table.Cell().Element(CellStyle).Padding(2).PaddingRight(5).AlignRight().Row(r =>
                    {
                        r.RelativeItem().Text("$");
                        r.RelativeItem(3).Text($"{subtotal:F2}").AlignRight();
                    });
                }

                table.Cell().ColumnSpan(2).Text(string.Empty);
                table.Cell().Element(CellStyle).Padding(2).PaddingRight(5).Text("SUBTOTAL").AlignCenter();
                table.Cell().Element(CellStyle).Padding(2).PaddingRight(5).AlignRight().Row(r =>
                {
                    r.RelativeItem().Text("$");
                    r.RelativeItem(3).Text($"{GetProforma.SubTotal:F2}").AlignRight();
                });
                table.Cell().ColumnSpan(2).Text(string.Empty);

                table.Cell().Element(CellStyle).Padding(2).PaddingRight(5).Text("COMISIÓN").AlignCenter();
                table.Cell().Element(CellStyle).Padding(2).PaddingRight(5).AlignRight().Row(r =>
                {
                    r.RelativeItem().Text("$");
                    r.RelativeItem(3).Text($"{GetProforma.Commission:F2}").AlignRight();
                });
                table.Cell().ColumnSpan(2).Text(string.Empty);

                var label = "SALDO A FAVOR";
                if (GetProforma.Discount < 0)
                {
                    label = "DEUDA";
                }

                table.Cell().Element(CellStyle).Padding(2).PaddingRight(5).Text(label).FontSize(10).AlignCenter();
                table.Cell().Element(CellStyle).Padding(2).PaddingRight(5).AlignRight().Row(r =>
                {
                    r.RelativeItem().Text("$");
                    if (GetProforma.Discount < 0)
                    {
                        r.RelativeItem(3).Text($"{-1 * GetProforma.Discount:F2}").AlignRight();
                    }
                    else
                    {
                        r.RelativeItem(3).Text($"({GetProforma.Discount:F2})").AlignRight();
                    }

                });
                table.Cell().ColumnSpan(2).Text(string.Empty);
                table.Cell().Element(CellStyle).Padding(2).PaddingRight(5).Text("TOTAL").Bold().AlignCenter();
                table.Cell().Element(CellStyle).Padding(2).PaddingRight(5).AlignRight().Row(r =>
                {
                    r.RelativeItem().Text("$");
                    r.RelativeItem(3).Text($"{GetProforma.Total:F2}").AlignRight();
                }); ;
            });

            static IContainer CellStyle(IContainer container)
            {
                return container.Border(1).BorderColor(Colors.Black);
            }
        }
    }
}
