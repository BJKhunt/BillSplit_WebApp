using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication12.Data;
using WebApplication12.Models;
using WebApplication12.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace WebApplication12.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;

        public ExpensesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
            _context = context;
        }

        // GET: Expenses
        public async Task<IActionResult> Index()
        {
            var currentUser = await userManager.GetUserAsync(User);
            return View(await _context.Expenses.Where(c => c.CurrentUser == currentUser.Id).ToListAsync());
        }

        
        // GET: Expenses/Create
        public async Task<IActionResult> CreateAsync()
        {
            var currentUser = await userManager.GetUserAsync(User);
            var emails = (IEnumerable<string>)_context.Friends.Where(c => c.CurrentUser == currentUser.Email).Select(c => c.FriendUser);
            List<ApplicationUser> friendList = new List<ApplicationUser>();
            foreach (string friendEmail in emails)
            {
                var applicationUser = await userManager.FindByEmailAsync(friendEmail);
                friendList.Add(applicationUser);
            }
            ViewBag.selectedList = new SelectList(friendList, "Id", "FirstName");
            return View();
        }

        // POST: Expenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense)
        {
            if (!ModelState.IsValid)
            {
                var currentUser = await userManager.GetUserAsync(User);
                var emails = (IEnumerable<string>)_context.Friends.Where(c => c.CurrentUser == currentUser.Email).Select(c => c.FriendUser);
                List<ApplicationUser> friendList = new List<ApplicationUser>();
                foreach (string friendEmail in emails)
                {
                    var applicationUser = await userManager.FindByEmailAsync(friendEmail);
                    friendList.Add(applicationUser);
                }
                ViewBag.selectedList = new SelectList(friendList, "Id", "FirstName");
                return View();
            }
            else
            {
                int tempData=-1;
                if (expense.IsOwe == 1)
                    tempData = 2;
                else
                    tempData = 1;
                Expense newOther = new Expense()
                {
                    CurrentUser = expense.FriendUser,
                    Note = expense.Note,
                    FriendUser = expense.CurrentUser,
                    FriendUserValue = expense.FriendUserValue,
                    IsOwe = tempData,
                };
                _context.Add(expense);
                _context.Add(newOther);
                var currentUser = await userManager.GetUserAsync(User);
                var otherUser = await userManager.FindByIdAsync(expense.FriendUser);
                if (expense.IsOwe==1)
                {
                    currentUser.Value -= expense.FriendUserValue;
                    otherUser.Value += expense.FriendUserValue;
                }
                else
                {
                    currentUser.Value += expense.FriendUserValue;
                    otherUser.Value -= expense.FriendUserValue;
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = _context.Expenses.Find(id);
            if (expense == null)
            {
                return NotFound();
            }
            var currentUser =await userManager.GetUserAsync(User);
            if (expense.IsOwe == 1)
            {
                currentUser.Value += expense.FriendUserValue;
            }
            else
            {
                currentUser.Value -= expense.FriendUserValue;
            }
            await _context.SaveChangesAsync();
            return View(expense);
        }

        // POST: Expenses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,CurrentUser,CurrentUserValue,Note,IsOwe,FriendUser,FriendUserValue")] Expense expense)
        {
            if (id != expense.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await userManager.GetUserAsync(User);
                    if (expense.IsOwe == 1)
                    {
                        currentUser.Value -= expense.FriendUserValue;
                    }
                    else
                    {
                        currentUser.Value += expense.FriendUserValue;
                    }

                    var otherExpense = await _context.Expenses.FirstOrDefaultAsync(m => m.id == (id + 1));
                    otherExpense.FriendUserValue = expense.FriendUserValue;
                    if (expense.IsOwe == 1)
                        otherExpense.IsOwe = 2;
                    else
                        otherExpense.IsOwe = 1;
                    _context.Update(otherExpense);
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(expense);
        }

        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(m => m.id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            var currentUser = await userManager.GetUserAsync(User);
            var otherExpense = await _context.Expenses.FirstOrDefaultAsync(m => m.id == (id + 1));
            if (expense.IsOwe == 1)
            {
                currentUser.Value += expense.FriendUserValue;

            }
            else
            {

                currentUser.Value -= expense.FriendUserValue;
            }
            _context.Expenses.Remove(otherExpense);
            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.id == id);
        }
    }
}
