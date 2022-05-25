using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArtContestClub.Data;
using ArtContestClub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ArtContestClub.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MessagesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
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

        public string GetRank(string userIdentity)
        {
            var personRank = _context.Ranks.FirstOrDefault(p => p.User == userIdentity && p.Expires > DateTime.Now && p.Name == "Admin");
            if (personRank == null) personRank = _context.Ranks.FirstOrDefault(p => p.User == userIdentity && p.Expires > DateTime.Now && p.Name == "Mod");
            if (personRank == null) personRank = _context.Ranks.FirstOrDefault(p => p.User == userIdentity && p.Expires > DateTime.Now && p.Name == "Banned");
            if (personRank == null) personRank = _context.Ranks.FirstOrDefault(p => p.User == userIdentity && p.Expires > DateTime.Now && p.Name == "Premium");
            if (personRank == null) personRank = _context.Ranks.FirstOrDefault(p => p.User == userIdentity && p.Expires > DateTime.Now && p.Name == "Vip");
            if (personRank == null)
            {
                return "User";
            }
            else
            {
                return personRank.Name;
            }
        }

        public int RankToNumber(string rank)
        {
            switch (rank)
            {
                case "Admin":
                    return 5;
                case "Mod":
                    return 4;
                case "Premium":
                    return 3;
                case "Vip":
                    return 2;
                case "User":
                    return 1;
                case "Banned":
                    return 0;
                default:
                    return 0;
            }
        }

        [Authorize]
        public async Task<IActionResult> View(string? messageSend)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (messageSend == "true")
            {
                ViewData["MessageSend"] = "true";
            }
            else
            {
                ViewData["MessageSend"] = "false";
            }

            var result = await _context.Messages
                .Where(p => p.To == _userManager.GetUserId(User) && p.IsDeleted == false)
                .OrderByDescending(p => p.CreatedDate).ToListAsync();

            if (result != null)
                foreach (var a in result)
                {
                    ViewData[a.From] = GetUsernameOrEmailFromUserIdentity(a.From.ToString());
                }

            return View(result);
        }

        [Authorize]
        public async Task<IActionResult> Support(string? messageTo)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) >= 4)
            {
                ViewData["Support"] = "true";
            }
            else ViewData["Support"] = "false";

            if (messageTo != null)
            {
                if (messageTo.Length > 50)  messageTo = messageTo.Substring(0, 50);
                ViewData["MessageTo"] = messageTo;
            }
            else
            {
                ViewData["MessageTo"] = "";
            }

            return View();


        }

        [Authorize]
        public async Task<IActionResult> Create(string? messageTo)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if(RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 1 && messageTo != "Support")
            {
                return RedirectToAction("YouAreBanned", "Ranks");
            }

            if (messageTo != null)
            {
                if (messageTo.Length > 50)  messageTo = messageTo.Substring(0, 50);

                ViewData["MessageTo"] = messageTo;
            }
            else
            {
                ViewData["MessageTo"] = "";
            }

            return View();


        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubmit([Bind("Id,Title,Content,CreatedDate,From,To,IsDeleted")] Message message)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            message.CreatedDate = DateTime.Now;
            message.IsDeleted = false;
            message.From = _userManager.GetUserId(User);
            if (message.To == null || message.Title == null || message.Content == null) return RedirectToAction("Create", message);

            if (message.To.Length > 50)  message.To = message.To.Substring(0, 50);
            if (message.Title.Length > 100)  message.Title = message.Title.Substring(0, 100);
            if (message.Content.Length > 500)  message.Content = message.Content.Substring(0, 500);


            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 1 && message.To != "Support")
            {
                return RedirectToAction("YouAreBanned", "Ranks");
            }

            if (true)
            {
                _context.Add(message);
                await _context.SaveChangesAsync();
                return Redirect("~/Messages/View?messageSend=true");
            }
            return RedirectToAction("Create", message);
        }

        // GET: Messages/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Messages == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (message == null || message.To != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            if (message != null)
            {
                message.IsDeleted = true;
                _context.Messages.Update(message);
            }

            await _context.SaveChangesAsync();

            // ViewData[message.From] = GetUsernameOrEmailFromUserIdentity(message.From.ToString());

            return RedirectToAction(nameof(View));
        }

        private bool MessageExists(int id)
        {
          return (_context.Messages?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
