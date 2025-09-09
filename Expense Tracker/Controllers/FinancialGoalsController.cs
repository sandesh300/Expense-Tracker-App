// In Controllers/FinancialGoalsController.cs
using Expense_Tracker.Data;
using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class FinancialGoalsController : Controller
{
    private readonly ApplicationDbContext _context;

    public FinancialGoalsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: FinancialGoals  
    public async Task<IActionResult> Index()
    {
        var goals = await _context.FinancialGoals.ToListAsync();
        return View(goals);
    }
}