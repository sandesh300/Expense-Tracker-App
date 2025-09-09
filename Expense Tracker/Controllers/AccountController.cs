using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker_App.Controllers
{
    public class AccountController : Controller
    {
        // This action will show the Login page UI
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // This action will show the Register page UI
        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST actions for handling form submissions will go here later
        // when you connect this to ASP.NET Core Identity.
    }
}