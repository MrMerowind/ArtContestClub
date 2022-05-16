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

namespace ArtContestClub.Controllers
{
    public class ContestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Contests
        public async Task<IActionResult> Index(int? page)
        {
            if (page == null || page < 0) page = 0;
            return View(await _context.Contests.Where(p => p.IsBanned == false && p.IsDeleted == false)
                .OrderBy(p => p.Id).Skip((int)page * 100).Take(100).ToListAsync());
        }

        public async Task<IActionResult> MyContests()
        {
            return View(await _context.Contests.Where(p => p.OwnerEmail == User.Identity.Name && p.IsDeleted == false).ToListAsync());
        }

        // GET: Contests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
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

        // GET: Contests/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Contests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,OwnerEmail,Title,Description,IsNsfw,IsDeleted,IsBanned,SkillLevel,MaxParticipants,CurrentParticipants,FirstPlaceUserEmail,SecondPlaceUserEmail,ThirdPlaceUserEmail,Created,Deadline,Branch")] Contest contest)
        {
            string? email = User.Identity.Name;
            if(email == null) return View(contest);
            else contest.OwnerEmail = email;

            if(contest.Title == "" || contest.Title == null
                || contest.Description == "" || contest.Description == null)
            {
                View(contest);
            }

            switch (contest.SkillLevel)
            {
                case "1":
                    contest.SkillLevel = "Newbie";
                    break;
                case "2":
                    contest.SkillLevel = "Beginner";
                    break;
                case "3":
                    contest.SkillLevel = "Medium";
                    break;
                case "4":
                    contest.SkillLevel = "Skilled";
                    break;
                case "5":
                    contest.SkillLevel = "Professional";
                    break;
                case "6":
                    contest.SkillLevel = "Art God";
                    break;
                case "7":
                    contest.SkillLevel = "All";
                    break;
                default:
                    return View(contest);
            }

            switch (contest.MaxParticipants)
            {
                case 10:
                    break;
                case 25:
                    break;
                case 50:
                    if(!(User.IsInRole("Premium") ||
                        User.IsInRole("Admin") ||
                        User.IsInRole("Mod")))
                    {
                        return View(contest);
                    }
                    break;
                case 100:
                    if (!(User.IsInRole("Premium") ||
                        User.IsInRole("Admin") ||
                        User.IsInRole("Mod")))
                    {
                        return View(contest);
                    }
                    break;
                case 250:
                    if (!(User.IsInRole("Admin") ||
                        User.IsInRole("Mod")))
                    {
                        return View(contest);
                    }
                    break;
                case 500:
                    if (!(User.IsInRole("Admin") ||
                        User.IsInRole("Mod")))
                    {
                        return View(contest);
                    }
                    break;
                case 1000:
                    if (!(User.IsInRole("Admin")))
                    {
                        return View(contest);
                    }
                    break;
                default:
                    return View(contest);
            }

            if(contest.Deadline <= DateTime.Now)
            {
                return View(contest);
            }

            switch (contest.Branch)
            {
                case "1":
                    contest.Branch = "Digital drawing";
                    break;
                case "2":
                    contest.Branch = "Digital painting";
                    break;
                case "3":
                    contest.Branch = "Traditional drawing";
                    break;
                case "4":
                    contest.Branch = "Traditional painting";
                    break;
                case "5":
                    contest.Branch = "Photography";
                    break;
                case "6":
                    contest.Branch = "3D";
                    break;
                case "7":
                    contest.Branch = "Other";
                    break;
                default:
                    return View(contest);
            }

            contest.ThirdPlaceUserEmail = null;
            contest.SecondPlaceUserEmail = null;
            contest.FirstPlaceUserEmail = null;
            contest.CurrentParticipants = 0;
            contest.Created = DateTime.Now;

            contest.IsBanned = false;
            contest.IsDeleted = false;
            if (ModelState.IsValid)
            {
                _context.Add(contest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(contest);
        }

        // GET: Contests/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contests == null)
            {
                return NotFound();
            }

            var contest = await _context.Contests.FindAsync(id);
            if (contest == null)
            {
                return NotFound();
            }
            return View(contest);
        }

        // POST: Contests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OwnerEmail,Title,Description,IsNsfw,IsDeleted,IsBanned,SkillLevel,MaxParticipants,CurrentParticipants,FirstPlaceUserEmail,SecondPlaceUserEmail,ThirdPlaceUserEmail,Created,Deadline,Branch")] Contest contest)
        {
            if (id != contest.Id)
            {
                return NotFound();
            }
            string? email = User.Identity.Name;
            if (email == null || email != contest.OwnerEmail) return View(contest);

            if (contest.Title == "" || contest.Title == null
                || contest.Description == "" || contest.Description == null)
            {
                View(contest);
            }

            switch (contest.SkillLevel)
            {
                case "1":
                    contest.SkillLevel = "Newbie";
                    break;
                case "2":
                    contest.SkillLevel = "Beginner";
                    break;
                case "3":
                    contest.SkillLevel = "Medium";
                    break;
                case "4":
                    contest.SkillLevel = "Skilled";
                    break;
                case "5":
                    contest.SkillLevel = "Professional";
                    break;
                case "6":
                    contest.SkillLevel = "Art God";
                    break;
                case "7":
                    contest.SkillLevel = "All";
                    break;
                default:
                    return View(contest);
            }

            switch (contest.MaxParticipants)
            {
                case 10:
                    break;
                case 25:
                    break;
                case 50:
                    if (!(User.IsInRole("Premium") ||
                        User.IsInRole("Admin") ||
                        User.IsInRole("Mod")))
                    {
                        return View(contest);
                    }
                    break;
                case 100:
                    if (!(User.IsInRole("Premium") ||
                        User.IsInRole("Admin") ||
                        User.IsInRole("Mod")))
                    {
                        return View(contest);
                    }
                    break;
                case 250:
                    if (!(User.IsInRole("Admin") ||
                        User.IsInRole("Mod")))
                    {
                        return View(contest);
                    }
                    break;
                case 500:
                    if (!(User.IsInRole("Admin") ||
                        User.IsInRole("Mod")))
                    {
                        return View(contest);
                    }
                    break;
                case 1000:
                    if (!(User.IsInRole("Admin")))
                    {
                        return View(contest);
                    }
                    break;
                default:
                    return View(contest);
            }

            switch (contest.Branch)
            {
                case "1":
                    contest.Branch = "Digital drawing";
                    break;
                case "2":
                    contest.Branch = "Digital painting";
                    break;
                case "3":
                    contest.Branch = "Traditional drawing";
                    break;
                case "4":
                    contest.Branch = "Traditional painting";
                    break;
                case "5":
                    contest.Branch = "Photography";
                    break;
                case "6":
                    contest.Branch = "3D";
                    break;
                case "7":
                    contest.Branch = "Other";
                    break;
                default:
                    return View(contest);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContestExists(contest.Id))
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
            return View(contest);
        }

        // GET: Contests/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contests == null)
            {
                return NotFound();
            }

            var contest = await _context.Contests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contest == null || contest.OwnerEmail != User.Identity.Name)
            {
                return NotFound();
            }

            return View(contest);


        }

        // POST: Contests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Temporary end
            return RedirectToAction(nameof(Index));

            if (_context.Contests == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contests'  is null.");
            }
            var contest = await _context.Contests.FindAsync(id);
            if (contest != null)
            {
                if(contest.OwnerEmail == User.Identity.Name)
                {
                    contest.IsDeleted = true;
                    try
                    {
                        _context.Update(contest);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ContestExists(contest.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    _context.Contests.Update(contest);
                }
                
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ContestExists(int id)
        {
          return (_context.Contests?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
