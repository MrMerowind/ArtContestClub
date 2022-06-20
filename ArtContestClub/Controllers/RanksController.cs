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
    public class RanksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RanksController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
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

        // GET: Ranks
        [Authorize]
        public async Task<IActionResult> BuyVipMonth()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> BuyVipYear()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> BuyPremiumMonth()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> BuyPremiumYear()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> OrderComplete(string? number)
        {
            if (number == null) return RedirectToAction("Index", "Ranks");
            if (number == "444048") // Vip30d
            {
                _context.Ranks.Add(new Rank()
                {
                    Name = "Vip",
                    CreateTime = DateTime.Now,
                    Expires = DateTime.Now.AddDays(30),
                    User = _userManager.GetUserId(User)
                });
            }
            else if(number == "716154") // Vip365d
            {
                _context.Ranks.Add(new Rank()
                {
                    Name = "Vip",
                    CreateTime = DateTime.Now,
                    Expires = DateTime.Now.AddDays(365),
                    User = _userManager.GetUserId(User)
                });
            }
            else if (number == "351914") // Premium30d
            {
                _context.Ranks.Add(new Rank()
                {
                    Name = "Premium",
                    CreateTime = DateTime.Now,
                    Expires = DateTime.Now.AddDays(30),
                    User = _userManager.GetUserId(User)
                });
            }
            else if (number == "892290") // Premium365d
            {
                _context.Ranks.Add(new Rank()
                {
                    Name = "Premium",
                    CreateTime = DateTime.Now,
                    Expires = DateTime.Now.AddDays(365),
                    User = _userManager.GetUserId(User)
                });
            }
            else if (number == "0") // Order complete
            {
                return RedirectToAction("Index", "Ranks");
            }

            await _context.SaveChangesAsync();
            return View();
            
        }



        [Authorize]
        public async Task<IActionResult> Index()
        {
            string userId = _userManager.GetUserId(User);
            int rank = RankToNumber(GetRank(userId));

            if(rank >= 5)
            {
                ViewData["CreateRank"] = "true";
            }
            else
            {
                ViewData["CreateRank"] = "false";
            }

            var ranks = await _context.Ranks.Where(p => p.User == userId).OrderByDescending(p => p.Expires).ToListAsync();


            return View(ranks);
        }

        [Authorize]
        public async Task<IActionResult> YouAreBanned()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 1)
            {
                ViewData["YouAreBanned"] = "true";
            }
            else
            {
                ViewData["YouAreBanned"] = "false";
            }

            return View();
        }

        // GET: Ranks/Create
        [Authorize]
        public IActionResult Create()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 5) return NotFound();

            return View();
        }

        // POST: Ranks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CreateTime,Expires,User")] Rank rank)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            rank.CreateTime = DateTime.Now;

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 5) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Add(rank);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rank);
        }

        // GET: Ranks/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            // Temporary disable
            return RedirectToAction(nameof(Index));

            if (id == null || _context.Ranks == null)
            {
                return NotFound();
            }

            var rank = await _context.Ranks.FindAsync(id);
            if (rank == null)
            {
                return NotFound();
            }
            return View(rank);
        }

        // POST: Ranks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreateTime,Expires,User")] Rank rank)
        {
            // Temporary disable
            return RedirectToAction(nameof(Index));

            if (id != rank.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rank);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RankExists(rank.Id))
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
            return View(rank);
        }

        // GET: Ranks/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            // Temporary disable
            return RedirectToAction(nameof(Index));

            if (id == null || _context.Ranks == null)
            {
                return NotFound();
            }

            var rank = await _context.Ranks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rank == null)
            {
                return NotFound();
            }

            return View(rank);
        }

        // POST: Ranks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Temporary disable
            return RedirectToAction(nameof(Index));

            if (_context.Ranks == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Ranks'  is null.");
            }
            var rank = await _context.Ranks.FindAsync(id);
            if (rank != null)
            {
                _context.Ranks.Remove(rank);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RankExists(int id)
        {
          return (_context.Ranks?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
