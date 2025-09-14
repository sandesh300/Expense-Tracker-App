// In Models/GoalContribution.cs
using Expense_Tracker.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker.Models
{
    public class GoalContribution
    {
        [Key]
        public int ContributionId { get; set; }

        [ForeignKey("FinancialGoal")]
        public int GoalId { get; set; }
        public FinancialGoal FinancialGoal { get; set; } // Navigation property

        [Range(0.01, double.MaxValue, ErrorMessage = "Contribution Amount must be greater than 0.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public DateTime ContributionDate { get; set; }

        // Optional: Link contribution to a specific income transaction
        [ForeignKey("Transaction")]
        public int? TransactionId { get; set; } // Nullable, as not all contributions come from a transaction
        public Transaction Transaction { get; set; }
    }
}