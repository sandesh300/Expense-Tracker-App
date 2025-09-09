using Expense_Tracker.Models;
using Microsoft.EntityFrameworkCore;



namespace Expense_Tracker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Transaction> Transactions { get; set; }    
        public DbSet<Category> Categories { get; set; }

        // In Models/ApplicationDbContext.cs
        public DbSet<FinancialGoal> FinancialGoals { get; set; }
        public DbSet<GoalContribution> GoalContributions { get; set; }

    }
}
