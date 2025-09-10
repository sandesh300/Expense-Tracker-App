using Expense_Tracker.Data;
using Expense_Tracker.Models;
using Expense_Tracker.Models;
using Expense_Tracker_App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // <-- Add these using statements
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Expense_Tracker.Controllers
{
    [Authorize] // <-- CRITICAL: Ensures only logged-in users can see the dashboard.

    public class DashboardController : Controller

    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // <-- CHANGED: Use ApplicationUser instead of IdentityUser
        private readonly IEmailService _emailService;


        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager; // <-- ADD THIS
            _emailService = emailService; // <-- Store the service

        }

        public async Task<IActionResult> Index()
        {
            // --- Get Current User's ID ---
            var userId = _userManager.GetUserId(User); // <-- GET USER ID FIRST

            //Date Filter
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            // --- Filter all database queries by UserId ---
            List<Transaction> SelectedTransactions = await _context.Transactions
                .Where(t => t.UserId == userId) // <-- ADD THIS FILTER
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDate)
                .ToListAsync();

            // Total Income
            decimal TotalIncome = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .Sum(j => j.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("C0");

            // Total Expense
            decimal TotalExpense = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .Sum(j => j.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("C0");

            // Balance
            decimal Balance = TotalIncome - TotalExpense;
            ViewBag.Balance = Balance.ToString("C0");

            // Doughnut Chart - Expense By Category
            ViewBag.DoughnutChartData = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
                })
                .OrderByDescending(l => l.amount)
                .ToList();


            // ==========================================================
            // === ADD THIS ENTIRE BLOCK FOR THE SPLINE CHART DATA ===
            // ==========================================================
            // Spline Chart - Income vs Expense
            // Income
            List<SplineChartData> IncomeSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    income = k.Sum(l => l.Amount)
                })
                .ToList();

            // Expense
            List<SplineChartData> ExpenseSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    expense = k.Sum(l => l.Amount)
                })
                .ToList();

            // Combine Income & Expense
            string[] Last7Days = Enumerable.Range(0, 7)
                .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            ViewBag.SplineChartData = from day in Last7Days
                                      join income in IncomeSummary on day equals income.day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new SplineChartData()
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense,
                                      };
            // ==========================================================
            // === END OF ADDED BLOCK ===
            // ==========================================================

            // Recent Transactions
            ViewBag.RecentTransactions = await _context.Transactions
                .Where(t => t.UserId == userId) // <-- ADD THIS FILTER
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();

            return View();
        }

        // Replace the existing DownloadReport method with this one.

        // In Controllers/DashboardController.cs

        [Authorize]
        public async Task<IActionResult> DownloadReport()
        {
            var username = _userManager.GetUserName(User);
            var userId = _userManager.GetUserId(User);

            // ==========================================================
            // === EFFICIENT DATA GATHERING LOGIC STARTS HERE ===
            // ==========================================================

            // Build the base query for the current user's transactions.
            // Notice we do NOT use ToListAsync() here. This is an IQueryable.
            var userTransactionsQuery = _context.Transactions
                .Where(t => t.UserId == userId)
                .Include(t => t.Category);

            // 1. GET RECENT TRANSACTIONS
            // The database query only runs here, fetching just 5 records.
            var recentTransactions = await userTransactionsQuery
                .OrderByDescending(t => t.Date)
                .Take(5)
                .ToListAsync();

            // 2. GET SUMMARY DATA
            // The database query only runs here, fetching aggregated results.
            var summaryData = await userTransactionsQuery
                .GroupBy(t => 1) // Group by a constant to aggregate all rows
                .Select(g => new
                {
                    TotalIncome = g.Where(t => t.Category.Type == "Income").Sum(t => (decimal?)t.Amount) ?? 0,
                    TotalExpense = g.Where(t => t.Category.Type == "Expense").Sum(t => (decimal?)t.Amount) ?? 0,
                }).FirstOrDefaultAsync();

            decimal totalIncome = summaryData?.TotalIncome ?? 0;
            decimal totalExpense = summaryData?.TotalExpense ?? 0;
            decimal balance = totalIncome - totalExpense;


            // 3. GET DOUGHNUT CHART DATA
            // The database query only runs here, fetching grouped category data.
            var doughnutChartData = await userTransactionsQuery
                .Where(t => t.Category.Type == "Expense")
                .GroupBy(t => new { t.CategoryId, t.Category.Icon, t.Category.Title })
                .Select(g => new DoughnutChartData
                {
                    CategoryTitleWithIcon = g.Key.Icon + " " + g.Key.Title,
                    Amount = g.Sum(t => t.Amount),
                    FormattedAmount = g.Sum(t => t.Amount).ToString("C0")
                })
                .OrderByDescending(d => d.Amount)
                .ToListAsync();

            // ==========================================================
            // === DATA GATHERING LOGIC ENDS HERE ===
            // ==========================================================


            // 4. POPULATE THE REPORT MODEL
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;

            var reportModel = new PdfReportModel
            {
                ReportGeneratedFor = username,
                TotalIncome = totalIncome.ToString("C0"),
                TotalExpense = totalExpense.ToString("C0"),
                Balance = String.Format(culture, "{0:C0}", balance),
                RecentTransactions = recentTransactions,
                ExpensesByCategory = doughnutChartData
            };

            // 5. GENERATE THE PDF
            var pdfBytes = PdfReportGenerator.Generate(reportModel);

            // 6. RETURN THE FILE
            return File(pdfBytes, "application/pdf", "ExpenseReport.pdf");
        }


        // ADD THIS NEW ACTION
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against CSRF attacks
        public async Task<IActionResult> SendReportEmail()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    return Json(new { success = false, message = "User email not found." });
                }

                // --- Step 1: Generate the PDF (reuse existing logic) ---
                // (This is the same data-gathering and PDF generation logic from your DownloadReport action)
                var reportModel = await GenerateReportModelForCurrentUser(); // Refactored logic
                var pdfBytes = PdfReportGenerator.Generate(reportModel);

                // --- Step 2: Send the Email ---
                var subject = "Your Expense Tracker Report";
                var htmlContent = $"<p>Hello {user.UserName},</p><p>Please find your expense report attached.</p>";
                var pdfFileName = $"ExpenseReport_{DateTime.Now:yyyy-MM-dd}.pdf";

                await _emailService.SendReportEmailAsync(user.Email, subject, htmlContent, pdfBytes, pdfFileName);

                return Json(new { success = true, message = $"Report successfully sent to {user.Email} !" });
            }
            catch (Exception ex)
            {
                // Log the exception ex
                return Json(new { success = false, message = "An error occurred while sending the email." });
            }
        }
        private async Task<PdfReportModel> GenerateReportModelForCurrentUser()
        {
            var username = _userManager.GetUserName(User);
            var userId = _userManager.GetUserId(User);

            // ==========================================================
            // === EFFICIENT DATA GATHERING LOGIC STARTS HERE ===
            // ==========================================================

            // Build the base query for the current user's transactions.
            // Notice we do NOT use ToListAsync() here. This is an IQueryable.
            var userTransactionsQuery = _context.Transactions
                .Where(t => t.UserId == userId)
                .Include(t => t.Category);

            // 1. GET RECENT TRANSACTIONS
            // The database query only runs here, fetching just 5 records.
            var recentTransactions = await userTransactionsQuery
                .OrderByDescending(t => t.Date)
                .Take(5)
                .ToListAsync();

            // 2. GET SUMMARY DATA
            // The database query only runs here, fetching aggregated results.
            var summaryData = await userTransactionsQuery
                .GroupBy(t => 1) // Group by a constant to aggregate all rows
                .Select(g => new
                {
                    TotalIncome = g.Where(t => t.Category.Type == "Income").Sum(t => (decimal?)t.Amount) ?? 0,
                    TotalExpense = g.Where(t => t.Category.Type == "Expense").Sum(t => (decimal?)t.Amount) ?? 0,
                }).FirstOrDefaultAsync();

            decimal totalIncome = summaryData?.TotalIncome ?? 0;
            decimal totalExpense = summaryData?.TotalExpense ?? 0;
            decimal balance = totalIncome - totalExpense;


            // 3. GET DOUGHNUT CHART DATA
            // The database query only runs here, fetching grouped category data.
            var doughnutChartData = await userTransactionsQuery
                .Where(t => t.Category.Type == "Expense")
                .GroupBy(t => new { t.CategoryId, t.Category.Icon, t.Category.Title })
                .Select(g => new DoughnutChartData
                {
                    CategoryTitleWithIcon = g.Key.Icon + " " + g.Key.Title,
                    Amount = g.Sum(t => t.Amount),
                    FormattedAmount = g.Sum(t => t.Amount).ToString("C0")
                })
                .OrderByDescending(d => d.Amount)
                .ToListAsync();

            // ==========================================================
            // === DATA GATHERING LOGIC ENDS HERE ===
            // ==========================================================


            // 4. POPULATE THE REPORT MODEL
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;

            return new PdfReportModel
            {
                ReportGeneratedFor = username,
                TotalIncome = totalIncome.ToString("C0"),
                TotalExpense = totalExpense.ToString("C0"),
                Balance = String.Format(culture, "{0:C0}", balance),
                RecentTransactions = recentTransactions,
                ExpensesByCategory = doughnutChartData
            };
        }
    }
    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;

    }
}