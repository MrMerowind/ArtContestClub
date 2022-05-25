using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArtContestClub.Data;
using ArtContestClub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ArtContestClub.Controllers
{
    public class FriendsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FriendsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public string GetUsernameOrEmailFromUserIdentity(string userIdentity)
        {
            if (userIdentity == "Support") return "Support";

            var person = _context.AboutMe.FirstOrDefault(p => p.UserIdentity == userIdentity);
            string result = "Username";
            if (person == null || (person.Fullname == null || person.Fullname == ""))
            {
                var person2 = _userManager.FindByIdAsync(userIdentity);
                if (person2 != null && person2.Result != null)
                {
                    result = person2.Result.UserName.Split('@')[0];
                }

            }
            else
            {
                result = person.Fullname;
            }
            return result;
        }

        // GET: Friends
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            var friends = await _context.Friends
                .Where(p => p.UserIdentity == _userManager.GetUserId(User) || p.FriendIdentity == _userManager.GetUserId(User))
                .ToListAsync();


            if (friends != null)
                foreach (var a in friends)
                {
                    ViewData[a.UserIdentity.ToString()] = GetUsernameOrEmailFromUserIdentity(a.UserIdentity.ToString());
                    ViewData[a.FriendIdentity.ToString()] = GetUsernameOrEmailFromUserIdentity(a.FriendIdentity.ToString());
                }

            return View(friends);
        }


        // GET: Friends/Create
        [Authorize]
        public async Task<IActionResult> Create(string? id)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (id == null || id == _userManager.GetUserId(User))
            {
                return RedirectToAction(nameof(Index));
            }

            var friend = _context.Friends
                .FirstOrDefault(p => p.UserIdentity == _userManager.GetUserId(User) && p.FriendIdentity == id);

            var userThatIsGonnaBeAFriend = await _userManager.FindByIdAsync(id);

            if (friend == null && userThatIsGonnaBeAFriend != null)
            {
                _context.Add(new Friend()
                {
                    UserIdentity = _userManager.GetUserId(User),
                    FriendIdentity = id,
                    TimeAdded = DateTime.Now
                });
                
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Friends/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(string? id)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (id == null || id == _userManager.GetUserId(User))
            {
                return RedirectToAction("Index");
            }

            if (id == null || _context.Friends == null)
            {
                return NotFound();
            }

            var friend = await _context.Friends
                .FirstOrDefaultAsync(m => m.FriendIdentity == id && m.UserIdentity == _userManager.GetUserId(User));
            if (friend == null)
            {
                return NotFound();
            }

            if (friend != null)
            {
                _context.Friends.Remove(friend);
            }

            await _context.SaveChangesAsync();


            return RedirectToAction("Index");
        }

        private bool FriendExists(int id)
        {
          return (_context.Friends?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
