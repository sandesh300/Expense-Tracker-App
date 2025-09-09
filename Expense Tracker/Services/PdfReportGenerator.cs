using System.Globalization;
// In Services/PdfReportGenerator.cs
using Expense_Tracker.Models;
using Expense_Tracker.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ScottPlot;
using System.IO;

namespace Expense_Tracker_App.Services
{
    public static class PdfReportGenerator
    {
        public static byte[] Generate(PdfReportModel data)
        {
            // Set up QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Page setup
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily(QuestPDF.Helpers.Fonts.Arial));

                    // Header
                    page.Header().Element(header => ComposeHeader(header, data.ReportGeneratedFor));

                    // Content
                    page.Content().Element(content => ComposeContent(content, data));

                    // Footer
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        private static void ComposeHeader(IContainer container, string username)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text($"Expense Report for {username}")
                        .SemiBold().FontSize(24).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
                    column.Item().Text(DateTime.Now.ToString("d MMMM yyyy"))
                        .SemiBold().FontSize(12).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                });
            });
        }

        private static void ComposeContent(IContainer container, PdfReportModel data)
        {
            container.PaddingVertical(20).Column(column =>
            {
                // Summary Numbers
                column.Item().Element(summary => ComposeSummary(summary, data));
                column.Item().PaddingTop(20).Element(chart => ComposeChart(chart, data));
                column.Item().PaddingTop(20).Element(table => ComposeTransactionsTable(table, data.RecentTransactions));
            });
        }


        private static void ComposeSummary(IContainer container, PdfReportModel data)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(150);
                    columns.RelativeColumn();
                });

                table.Cell().Element(CellStyle).Text("Total Income").Bold();
                table.Cell().Element(CellStyle).Text(FormatValue(data.TotalIncome));

                table.Cell().Element(CellStyle).Text("Total Expense").Bold();
                table.Cell().Element(CellStyle).Text(FormatValue(data.TotalExpense));

                table.Cell().Element(CellStyle).Text("Balance").Bold();
                table.Cell().Element(CellStyle).Text(FormatValue(data.Balance));

                static IContainer CellStyle(IContainer c) =>
                    c.Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5);
            });
        }

        private static string FormatValue(object value)
        {
            if (value is decimal dec)
                return dec.ToString("C", CultureInfo.CurrentCulture);
            return value?.ToString() ?? "0";
        }


        private static void ComposeChart(IContainer container, PdfReportModel data)
        {
            // Generate the chart image using ScottPlot
            var plot = new Plot(600, 400);
            double[] values = data.ExpensesByCategory.Select(x => (double)x.Amount).ToArray();
            string[] labels = data.ExpensesByCategory.Select(x => x.CategoryTitleWithIcon.Split(' ')[0]).ToArray();

            if (values.Any())
            {
                plot.AddPie(values);
                plot.Legend(true, Alignment.MiddleRight);
            }
            else
            {
                plot.AddAnnotation("No expense data available", 10, 10);
            }

            plot.Title("Expenses by Category");

            // Convert the plot to a byte array (image)
            byte[] chartImageBytes = plot.GetImageBytes();

            // Place the image in the PDF
            container.AlignCenter().Image(chartImageBytes, ImageScaling.FitArea);
        }

        private static void ComposeTransactionsTable(IContainer container, List<Transaction> transactions)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Category").Bold();
                    header.Cell().Element(CellStyle).Text("Date").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Amount").Bold();

                    static IContainer CellStyle(IContainer c) =>
                        c.Padding(5).Background(Colors.Grey.Lighten3); // ✅ Background applied correctly
                });


                foreach (var transaction in transactions)
                {
                    table.Cell().Element(CellStyle).Text(transaction.Category.Title);
                    table.Cell().Element(CellStyle).Text(transaction.Date.ToString("d MMM yyyy"));
                    table.Cell().Element(CellStyle).AlignRight().Text(transaction.FormattedAmount);

                    static IContainer CellStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                }
            });
        }
    }
}