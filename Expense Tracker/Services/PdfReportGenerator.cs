using System.Globalization;
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
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    // Header
                    page.Header().Element(header => ComposeHeader(header, data.ReportGeneratedFor));

                    // Content
                    page.Content().Element(content => ComposeContent(content, data));

                    // Footer with date + page number
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text($"Generated on {DateTime.Now:d MMM yyyy}")
                            .FontSize(9).FontColor(Colors.Grey.Medium);

                        row.RelativeItem().AlignRight().Text($"{DateTime.Now:HH:mm} | Page ")
                            .FontSize(9).FontColor(Colors.Grey.Medium);

                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
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
                    column.Item().Text($"📊 Expense Report for {username}")
                        .SemiBold().FontSize(22).FontColor(Colors.Blue.Medium);
                    column.Item().Text($"Period: {DateTime.Now:MMMM yyyy}")
                        .FontSize(12).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private static void ComposeContent(IContainer container, PdfReportModel data)
        {
            container.PaddingVertical(20).Column(column =>
            {
                // Section: Summary
                column.Item().Text("📌 Summary Overview")
                    .FontSize(14).SemiBold().FontColor(Colors.Black);
                column.Item().PaddingBottom(15).Element(summary => ComposeSummary(summary, data));

                // Section: Chart
                column.Item().Text("📊 Expenses by Category")
                    .FontSize(14).SemiBold().FontColor(Colors.Black);
                column.Item().PaddingBottom(15).Element(chart => ComposeChart(chart, data));

                // Section: Recent Transactions
                column.Item().Text("📝 Recent Transactions")
                    .FontSize(14).SemiBold().FontColor(Colors.Black);
                column.Item().Element(table => ComposeTransactionsTable(table, data.RecentTransactions));
            });
        }

        private static void ComposeSummary(IContainer container, PdfReportModel data)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(180);
                    columns.RelativeColumn();
                });

                // Header styling
                table.Cell().Element(CellStyleHeader).Text("Metric").Bold();
                table.Cell().Element(CellStyleHeader).Text("Value").Bold();

                // Income
                table.Cell().Element(CellStyleBody).Text("💰 Total Income").Bold();
                table.Cell().Element(CellStyleBody).Text(FormatValue(data.TotalIncome));

                // Expense
                table.Cell().Element(CellStyleBody).Text("💸 Total Expense").Bold();
                table.Cell().Element(CellStyleBody).Text(FormatValue(data.TotalExpense));

                // Balance
                table.Cell().Element(CellStyleBody).Text("📉 Balance").Bold();
                table.Cell().Element(CellStyleBody).Text(FormatValue(data.Balance));

                static IContainer CellStyleHeader(IContainer c) =>
                    c.Background(Colors.Grey.Lighten3).Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Medium);

                static IContainer CellStyleBody(IContainer c) =>
                    c.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
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
            var plot = new Plot(600, 400);
            double[] values = data.ExpensesByCategory.Select(x => (double)x.Amount).ToArray();
            string[] labels = data.ExpensesByCategory.Select(x => x.CategoryTitleWithIcon).ToArray();

            if (values.Any())
            {
                var pie = plot.AddPie(values);
                pie.SliceLabels = labels; // ✅ Show category labels
                pie.ShowPercentages = true; // ✅ Show %
                pie.DonutSize = 0.5;        // ✅ Make it donut chart for clarity
                plot.Legend(true, Alignment.MiddleRight);
            }
            else
            {
                plot.AddAnnotation("No expense data available", 10, 10);
            }

            plot.Title("Expenses by Category", size: 16);

            byte[] chartImageBytes = plot.GetImageBytes();
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

                // Header row
                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Category").Bold();
                    header.Cell().Element(CellHeader).Text("Date").Bold();
                    header.Cell().Element(CellHeader).AlignRight().Text("Amount").Bold();

                    static IContainer CellHeader(IContainer c) =>
                        c.Padding(5).Background(Colors.Grey.Lighten3);
                });

                // Body rows
                foreach (var transaction in transactions)
                {
                    table.Cell().Element(CellBody).Text(transaction.Category.Title);
                    table.Cell().Element(CellBody).Text(transaction.Date.ToString("d MMM yyyy"));
                    table.Cell().Element(CellBody).AlignRight().Text(transaction.FormattedAmount);

                    static IContainer CellBody(IContainer c) =>
                        c.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                }
            });
        }
    }
}