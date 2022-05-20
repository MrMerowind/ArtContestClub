using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArtContestClub.Data;
using ArtContestClub.Models;

namespace ArtContestClub.Controllers
{
    public class AboutMeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AboutMeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AboutMe
        public async Task<IActionResult> Index()
        {
              return _context.AboutMe != null ? 
                          View(await _context.AboutMe.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.AboutMe'  is null.");
        }

        // GET: AboutMe/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.AboutMe == null)
            {
                return NotFound();
            }

            var aboutMe = await _context.AboutMe
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aboutMe == null)
            {
                return NotFound();
            }

            return View(aboutMe);
        }

        // GET: AboutMe/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AboutMe/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserIdentity,Fullname,Caption,Bio")] AboutMe aboutMe)
        {
            if (ModelState.IsValid)
            {
                _context.Add(aboutMe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(aboutMe);
        }

        // GET: AboutMe/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AboutMe == null)
            {
                return NotFound();
            }

            var aboutMe = await _context.AboutMe.FindAsync(id);
            if (aboutMe == null)
            {
                return NotFound();
            }
            return View(aboutMe);
        }

        // POST: AboutMe/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserIdentity,Fullname,Caption,Bio")] AboutMe aboutMe)
        {
            if (id != aboutMe.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aboutMe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AboutMeExists(aboutMe.Id))
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
            return View(aboutMe);
        }

        // GET: AboutMe/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AboutMe == null)
            {
                return NotFound();
            }

            var aboutMe = await _context.AboutMe
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aboutMe == null)
            {
                return NotFound();
            }

            return View(aboutMe);
        }

        // POST: AboutMe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AboutMe == null)
            {
                return Problem("Entity set 'ApplicationDbContext.AboutMe'  is null.");
            }
            var aboutMe = await _context.AboutMe.FindAsync(id);
            if (aboutMe != null)
            {
                _context.AboutMe.Remove(aboutMe);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AboutMeExists(int id)
        {
          return (_context.AboutMe?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
