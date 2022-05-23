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

        // GET: Messages/Create
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

            if (messageTo != null)
            {
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

            if (messageTo != null)
            {
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
            message.CreatedDate = DateTime.Now;
            message.IsDeleted = false;
            message.From = _userManager.GetUserId(User);
            if (message.To == null || message.Title == null || message.Content == null) return RedirectToAction("Create", message);
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

            ViewData[message.From] = GetUsernameOrEmailFromUserIdentity(message.From.ToString());

            return RedirectToAction(nameof(View));
        }

        private bool MessageExists(int id)
        {
          return (_context.Messages?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
