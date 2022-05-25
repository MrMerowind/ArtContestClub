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
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SupportController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
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

        // GET: Support
        [Authorize]
        public async Task<IActionResult> Index(string messageSend = "false")
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 4) return NotFound();

            if (messageSend == "true")
            {
                ViewData["MessageSend"] = "true";
            }
            else
            {
                ViewData["MessageSend"] = "false";
            }

            var result = await _context.Messages
                .Where(p => p.To == "Support" && p.IsDeleted == false)
                .OrderByDescending(p => p.CreatedDate).ToListAsync();

            if (result != null)
                foreach (var a in result)
                {
                    ViewData[a.From] = GetUsernameOrEmailFromUserIdentity(a.From.ToString());
                }

            return View(result);
        }

        // GET: Support/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            return NotFound();

            if (id == null || _context.Contests == null)
            {
                return NotFound();
            }

            var contest = await _context.Contests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contest == null)
            {
                return NotFound();
            }

            return View(contest);
        }

        // GET: Support/Create
        public IActionResult CreateMessage(string? messageTo)
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
                if (messageTo.Length > 50) messageTo = messageTo.Substring(0, 50);

                ViewData["MessageTo"] = messageTo;
            }
            else
            {
                ViewData["MessageTo"] = "";
            }

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 4) return NotFound();

            return View();
        }

        // POST: Support/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupportMessageSubmit([Bind("Id,Title,Content,CreatedDate,From,To,IsDeleted")] Message message)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 4) return NotFound();

            message.CreatedDate = DateTime.Now;
            if (message.Title == null) message.Title = "Message from support";
            message.IsDeleted = false;
            message.From = "Support";

            if (true)
            {
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize]
        public async Task<IActionResult> BanUser(string? userId, string? howLong)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (howLong == null || userId == null) return NotFound();

            var personToBan = await _userManager.FindByIdAsync(userId);
            if (personToBan == null) return NotFound();
            DateTime banLonginess = DateTime.Now;
            if (howLong == "1") banLonginess = DateTime.Now.AddDays(1);
            else if (howLong == "2") banLonginess = DateTime.Now.AddDays(7);
            else if (howLong == "3") banLonginess = DateTime.Now.AddDays(30);
            else banLonginess = DateTime.Now;

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 4) return NotFound();

            Rank? banRank = new Rank()
            {
                Name = "Banned",
                CreateTime = DateTime.Now,
                Expires = banLonginess,
                User = userId
            };

            _context.Ranks.Add(banRank);
            await _context.SaveChangesAsync();

            return Redirect("~/AboutMe/Index?id=" + userId);
        }

        [Authorize]
        public async Task<IActionResult> UnbanUser(string? userId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (userId == null) return NotFound();

            var personToUnban = await _userManager.FindByIdAsync(userId);
            if (personToUnban == null) return NotFound();

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 4) return NotFound();

            var bans = await _context.Ranks.Where(p => p.User == userId && p.Name == "Banned" && p.Expires > DateTime.Now).ToListAsync();

            _context.Ranks.RemoveRange(bans);

            await _context.SaveChangesAsync();
            return Redirect("~/AboutMe/Index?id=" + userId);
        }
        [Authorize]
        public async Task<IActionResult> GrantMod(string? userId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (userId == null) return NotFound();

            var personToGrantMod = await _userManager.FindByIdAsync(userId);
            if (personToGrantMod == null) return NotFound();


            DateTime modLonginess = DateTime.Now.AddYears(100); 

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 5) return NotFound();

            Rank? modRank = new Rank()
            {
                Name = "Mod",
                CreateTime = DateTime.Now,
                Expires = modLonginess,
                User = userId
            };

            _context.Ranks.Add(modRank);
            await _context.SaveChangesAsync();

            return Redirect("~/AboutMe/Index?id=" + userId);
        }
        [Authorize]
        public async Task<IActionResult> RemoveMod(string? userId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (userId == null) return NotFound();

            var personToRemoveMod = await _userManager.FindByIdAsync(userId);
            if (personToRemoveMod == null) return NotFound();

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 5) return NotFound();

            var mods = await _context.Ranks.Where(p => p.User == userId && p.Name == "Mod" && p.Expires > DateTime.Now).ToListAsync();

            _context.Ranks.RemoveRange(mods);

            await _context.SaveChangesAsync();
            return Redirect("~/AboutMe/Index?id=" + userId);
        }

        [Authorize]
        public async Task<IActionResult> BanContest(int? contestId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (contestId == null) return NotFound();

            var contestToBan = await _context.Contests.FirstOrDefaultAsync(p => p.Id == contestId);
            if (contestToBan == null) return NotFound();


            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 4) return NotFound();

            contestToBan.IsBanned = true;

            _context.Contests.Update(contestToBan);
            await _context.SaveChangesAsync();

            return Redirect("~/Contests/Details?id=" + contestId);
        }

        [Authorize]
        public async Task<IActionResult> UnbanContest(int? contestId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (contestId == null) return NotFound();

            var contestToUnban = await _context.Contests.FirstOrDefaultAsync(p => p.Id == contestId);
            if (contestToUnban == null) return NotFound();


            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 4) return NotFound();

            contestToUnban.IsBanned = false;

            _context.Contests.Update(contestToUnban);
            await _context.SaveChangesAsync();

            return Redirect("~/Contests/Details?id=" + contestId);
        }

        [Authorize]
        public async Task<IActionResult> BanSubmission(int? id, string? contestId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (id == null) return NotFound();

            var submissionToBan = await _context.ContestSubmissions.FirstOrDefaultAsync(p => p.Id == id);
            if (submissionToBan == null) return NotFound();

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 4) return NotFound();

            submissionToBan.IsBanned = true;

            _context.ContestSubmissions.Update(submissionToBan);
            await _context.SaveChangesAsync();

            return Redirect("~/Contests/Details?id=" + contestId);
        }

        [Authorize]
        public async Task<IActionResult> UnbanSubmission(int? id, string? contestId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (id == null) return NotFound();

            var submissionToBan = await _context.ContestSubmissions.FirstOrDefaultAsync(p => p.Id == id);
            if (submissionToBan == null) return NotFound();

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 4) return NotFound();

            submissionToBan.IsBanned = false;

            _context.ContestSubmissions.Update(submissionToBan);
            await _context.SaveChangesAsync();

            return Redirect("~/Contests/Details?id=" + contestId);
        }

        private bool ContestExists(int id)
        {
          return (_context.Contests?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
