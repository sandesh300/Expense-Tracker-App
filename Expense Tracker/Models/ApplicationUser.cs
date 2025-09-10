using Microsoft.AspNetCore.Identity;

namespace Expense_Tracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }   // New column for first name
    }
}
