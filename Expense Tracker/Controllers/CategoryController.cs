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
using Microsoft.AspNetCore.Authorization; // <-- IMPORTANT: Add this for security

namespace Expense_Tracker.Controllers
{
    [Authorize] // <-- CRITICAL: Ensures only logged-in users can access this controller.
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CategoryController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var userCategories = await _context.Categories
                .Where(c => c.UserId == userId) // <-- ADD THIS FILTER
                .ToListAsync();

            return View(userCategories);
        }

        // GET: Category/AddOrEdit/5
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return View(new Category());
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId); // <-- SECURE QUERY

                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
        }

        // POST: Category/AddOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("CategoryId,Title,Icon,Type")] Category category)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);

                // --- CREATE LOGIC ---
                if (category.CategoryId == 0)
                {
                    category.UserId = userId; // <-- CRITICAL STEP FOR CREATING
                    _context.Add(category);
                }
                // --- UPDATE LOGIC ---
                else
                {
                    var categoryInDb = await _context.Categories
                        .FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId && c.UserId == userId);

                    if (categoryInDb == null)
                    {
                        return NotFound(); // Security: User is trying to edit a category that isn't theirs.
                    }

                    categoryInDb.Title = category.Title;
                    categoryInDb.Icon = category.Icon;
                    categoryInDb.Type = category.Type;

                    _context.Update(categoryInDb);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId); // <-- SECURE QUERY

            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            else
            {
                return NotFound(); // Category not found or doesn't belong to the user.
            }

            return RedirectToAction(nameof(Index));
        }
    }
}