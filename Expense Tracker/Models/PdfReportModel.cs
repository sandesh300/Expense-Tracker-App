// In Models/PdfReportModel.cs
namespace Expense_Tracker.Models
{
    public class PdfReportModel
    {
        public string ReportGeneratedFor { get; set; } = "Guest";
        public string TotalIncome { get; set; }
        public string TotalExpense { get; set; }
        public string Balance { get; set; }

        // --- CHANGE THIS ---
        // Initialize the lists to guarantee they are never null.
        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
        public List<DoughnutChartData> ExpensesByCategory { get; set; } = new List<DoughnutChartData>();
    }
    // Helper class to hold chart data, you might already have this from your dashboard
    public class DoughnutChartData
    {
        public string CategoryTitleWithIcon { get; set; }
        public decimal Amount { get; set; }
        public string FormattedAmount { get; set; }
    }
}