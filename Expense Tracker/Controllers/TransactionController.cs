using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Data;
using Expense_Tracker.Models;

namespace Expense_Tracker.Controllers
{

    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; // <-- ADD THIS


        public TransactionController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager; // <-- ADD THIS

        }

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User); // Get current user's ID
            var applicationDbContext = _context.Transactions
                .Where(t => t.UserId == userId) // <-- ADD THIS FILTER
                .Include(t => t.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // In TransactionController.cs

        // GET: Transaction/AddOrEdit/5
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            PopulateCategories();

            // Case 1: This is for a new transaction
            if (id == 0)
                return View(new Transaction());

            // Case 2: This is for editing an existing transaction
            else
            {
                var userId = _userManager.GetUserId(User);
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId); // <-- SECURE QUERY

                if (transaction == null)
                {
                    return NotFound(); // If transaction doesn't exist or doesn't belong to user, return 404
                }

                return View(transaction);
            }
        }

        // POST: Transaction/AddOrEdit
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // In TransactionController.cs

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("TransactionId,CategoryId,Amount,Note,Date")] Transaction transaction)
        {
            // First, always run this to populate the dropdown in case of an error
            PopulateCategories();

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);

                // --- THIS IS THE CREATE LOGIC ---
                if (transaction.TransactionId == 0)
                {
                    // Stamp the new transaction with the current user's ID
                    transaction.UserId = userId; // <-- CRITICAL STEP FOR CREATING
                    _context.Add(transaction);
                }
                // --- THIS IS THE UPDATE LOGIC ---
                else
                {
                    // First, find the transaction in the DB, ensuring it belongs to the current user.
                    var transactionInDb = await _context.Transactions
                        .FirstOrDefaultAsync(t => t.TransactionId == transaction.TransactionId && t.UserId == userId);

                    if (transactionInDb == null)
                    {
                        return NotFound(); // Security: Don't let users edit data that isn't theirs.
                    }

                    // Update the properties of the transaction found in the database.
                    transactionInDb.CategoryId = transaction.CategoryId;
                    transactionInDb.Amount = transaction.Amount;
                    transactionInDb.Note = transaction.Note;
                    transactionInDb.Date = transaction.Date;

                    _context.Update(transactionInDb);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(transaction);
        }


        // POST: Transaction/Delete/5
        // In TransactionController.cs

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Find the transaction ensuring it belongs to the current user
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId); // <-- SECURE QUERY

            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Optional: Handle the case where the transaction was not found or didn't belong to the user
                // You could add a tempdata error message here.
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }


        [NonAction]
        public void PopulateCategories()
        {
            // Get the current user's ID first
            var userId = _userManager.GetUserId(User);

            // Filter the categories by this user ID
            var CategoryCollection = _context.Categories
                .Where(c => c.UserId == userId) // <-- ADD THIS FILTER
                .ToList();

            Category DefaultCategory = new Category() { CategoryId = 0, Title = "Choose a Category" };
            CategoryCollection.Insert(0, DefaultCategory);
            ViewBag.Categories = CategoryCollection;
        }
    }
}
