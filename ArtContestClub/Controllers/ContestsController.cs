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
        public async Task<IActionResult> Index()
        {
            return View(await _context.Contests.ToListAsync());
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,OwnerEmail,Description,IsNsfw,IsDeleted,IsBanned,SkillLevel,MaxParticipants,CurrentParticipants,FirstPlaceUserEmail,SecondPlaceUserEmail,ThirdPlaceUserEmail,Created,Deadline,Branch")] Contest contest)
        {
            if (id != contest.Id)
            {
                return NotFound();
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
        public async Task<IActionResult> Delete(int? id)
        {
            // Temporary end
            return RedirectToAction(nameof(Index));

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

        // POST: Contests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
                _context.Contests.Remove(contest);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContestExists(int id)
        {
          return (_context.Contests?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
