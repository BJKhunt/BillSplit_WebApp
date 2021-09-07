using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication12.Data;
using WebApplication12.Models;
using WebApplication12.ViewModels;

namespace WebApplication12.Controllers
{
    public class FriendsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        public FriendsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            this.userManager = userManager;
        }

        // GET: Friends
        public async Task<IActionResult> Index()
        {
            var currentUser = await userManager.GetUserAsync(User);
            var emails = (IEnumerable<string>)_context.Friends.Where(c => c.CurrentUser == currentUser.Email).Select(c => c.FriendUser);
            IList<ApplicationUser> friendList = new List<ApplicationUser>();
            foreach (string friendEmail in emails)
            {
                var applicationUser = await userManager.FindByEmailAsync(friendEmail);
                friendList.Add(applicationUser);
            }
            IList<FriendExpenseViewModel> fevm = new List<FriendExpenseViewModel>();
            foreach(var item in friendList)
            {
                float result = 0;
                var frinedExpenseList = _context.Expenses.Where(c => c.CurrentUser == currentUser.Id && c.FriendUser == item.Id).ToList();
                foreach(var item1 in frinedExpenseList)
                {
                    if(item1.IsOwe == 1)
                    {
                        result -= item1.FriendUserValue;
                    }
                    else
                    {
                        result += item1.FriendUserValue;
                    }
                }
                FriendExpenseViewModel dummy = new FriendExpenseViewModel()
                {
                    Id = item.Id,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    PhoneNumber = item.PhoneNumber,
                    Email = item.Email,
                    ExpenseValue = result
                };
                fevm.Add(dummy);
            }
            return View(fevm);
        }

        // GET: Friends/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Friends/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Friend friend)
        {
            if (ModelState.IsValid)
            {
                Friend newOther = new Friend()
                {
                    CurrentUser = friend.FriendUser,
                    FriendUser = friend.CurrentUser,
                };
                _context.Friends.Add(friend);
                _context.Friends.Add(newOther);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(friend);
        }


        // GET: Friends/Delete/5
        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            return View();
        }

        // POST: Friends/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var friendUser = await userManager.FindByIdAsync(id);
            var currentUser = await userManager.GetUserAsync(User);

            var friend = (Friend)_context.Friends.Where(c => c.CurrentUser == currentUser.Email && c.FriendUser == friendUser.Email).FirstOrDefault();
            var otherFriend = _context.Friends.Where(c => c.CurrentUser == friend.FriendUser && c.FriendUser == friend.CurrentUser).FirstOrDefault();
            _context.Friends.Remove(otherFriend);
            _context.Friends.Remove(friend);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
