// In Models/FinancialGoal.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker.Models
{
    public class FinancialGoal
    {
        [Key]
        public int GoalId { get; set; }

        [Required(ErrorMessage = "Goal Name is required.")]
        public string GoalName { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Target Amount must be greater than 0.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TargetAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CurrentAmount { get; set; } = 0; // Default to 0

        public DateTime TargetDate { get; set; }

        // We can add a relationship to a user later if you have user accounts.
        // public string UserId { get; set; }

        // Navigation property for all contributions to this goal
        public virtual ICollection<GoalContribution> GoalContributions { get; set; }
    }
}