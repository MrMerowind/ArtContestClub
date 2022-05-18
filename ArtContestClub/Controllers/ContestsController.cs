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
        /*
        public async Task<IActionResult> Index(int? page)
        {
            if (page == null || page < 0) page = 0;
            return View(await _context.Contests.Where(p => p.IsBanned == false && p.IsDeleted == false)
                .OrderBy(p => p.Id).Skip((int)page * 100).Take(100).ToListAsync());
        }
        */

        public async Task<IActionResult> MyContests()
        {
            return View(await _context.Contests.Where(p => p.OwnerEmail == User.Identity.Name && p.IsDeleted == false).ToListAsync());
        }

        public IActionResult SearchContest()
        {
            return View();
        }
        public async Task<IActionResult> Index(string? Title, bool? IsNsfw, string SkillLevel, string Branch, int page = 0)
        {
            var searchResult = _context.Contests.AsQueryable();
            if (Title == null) Title = "";
            if (IsNsfw == null) IsNsfw = false;
            if (SkillLevel == null || Branch == null) return View();
            if (page <= 0) page = 0;

            if (Title != "")
            {
                searchResult = searchResult.Where(p => p.Title.Contains(Title));
            }

            switch (SkillLevel)
            {
                case "1":
                    SkillLevel = "Newbie";
                    break;
                case "2":
                    SkillLevel = "Beginner";
                    break;
                case "3":
                    SkillLevel = "Medium";
                    break;
                case "4":
                    SkillLevel = "Skilled";
                    break;
                case "5":
                    SkillLevel = "Professional";
                    break;
                case "6":
                    SkillLevel = "Art God";
                    break;
                case "7":
                    SkillLevel = "All";
                    break;
                case "8":
                    SkillLevel = "Any";
                    break;
                default:
                    break;
            }

            if (SkillLevel != "Any" && SkillLevel != "")
            {
                searchResult = searchResult.Where(p => p.SkillLevel.Equals(SkillLevel));
            }

            if (IsNsfw == false)
            {
                searchResult = searchResult.Where(p => p.IsNsfw == false);
            }

            searchResult = searchResult.Where(p => p.CurrentParticipants < p.MaxParticipants);

            searchResult = searchResult.Where(p => p.Deadline > DateTime.Now);


            switch (Branch)
            {
                case "1":
                    Branch = "Digital drawing";
                    break;
                case "2":
                    Branch = "Digital painting";
                    break;
                case "3":
                    Branch = "Traditional drawing";
                    break;
                case "4":
                    Branch = "Traditional painting";
                    break;
                case "5":
                    Branch = "Photography";
                    break;
                case "6":
                    Branch = "3D";
                    break;
                case "7":
                    Branch = "Other";
                    break;
                case "8":
                    Branch = "Any";
                    break;
                default:
                    break;
            }

            if (Branch != "Any" && Branch != "")
            {
                searchResult = searchResult.Where(p => p.Branch.Equals(Branch));
            }


            searchResult = searchResult.Where(p => p.IsDeleted.Equals(false));

            searchResult = searchResult.Where(p => p.IsBanned.Equals(false) || (p.IsBanned == true && p.OwnerEmail == User.Identity.Name));

            searchResult = searchResult.Skip(100 * page);
            
            searchResult = searchResult.Take(100);

            return View(await searchResult.ToListAsync());

        }

        // GET: Contests/Details/5
        public async Task<IActionResult> Details(int id, int alreadySubmitted = 0, int reportContest = 0)
        {
            if (id == null || _context.Contests == null)
            {
                return NotFound();
            }

            var contest = await _context.Contests.FirstOrDefaultAsync(m => m.Id == id);
            if (contest == null)
            {
                return NotFound();
            }

            if (reportContest == 0)
            {
                ViewData["reportContest"] = "false";
            }
            else
            {
                ViewData["reportContest"] = "true";
            }

            if(alreadySubmitted == 1)
            {
                ViewData["alreadySubmitted"] = "true";
            }
            else
            {
                ViewData["alreadySubmitted"] = "false";
            }

            contest.ContestParticipants = await _context.ContestParticipants
                .Where(p => p.ParticipantEmail == User.Identity.Name && p.ContestId == id).ToListAsync();

            contest.ContestSubmissions = await _context.ContestSubmissions.Where(p => p.ContestId == id && p.IsDeleted == false
            && (p.IsBanned == false || (p.IsBanned == true && p.Username == User.Identity.Name))).ToListAsync(); 

            return View(contest);
        }

        // GET: Contests/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        public IActionResult SubmitArt(int id)
        {
            return View(new ContestSubmission() { Id = id });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> SubmitArtConfirm(string? Title, string? ArtLink, int? ContestId)
        {
            if (Title == null || ArtLink == null || ContestId == null) return Redirect("~/Contests/Details?id=" + ContestId);
            ContestSubmission contestSubmission = new ContestSubmission()
            {
                Title = Title,
                ArtLink = ArtLink,
                Username = User.Identity.Name,
                ContestId = ContestId,
                Submited = DateTime.Now,
                IsDeleted = false,
                IsBanned = false,
            };

            var contest = _context.Contests.FirstOrDefault(m => m.Id == ContestId);
            if (contest != null && contest.IsDeleted == false && contest.IsBanned == false &&
                (contest.Deadline == null || (contest.Deadline != null && contest.Deadline > DateTime.Now)) )
            {
                var previousSubmission = _context.ContestSubmissions
                .FirstOrDefault(p => p.ContestId == ContestId && p.Username == User.Identity.Name);
                
                if(previousSubmission == null)
                {
                    _context.ContestSubmissions.Add(contestSubmission);
                }
                else
                {
                    _context.ContestSubmissions.Remove(previousSubmission);
                    _context.ContestSubmissions.Add(contestSubmission);
                }
                await _context.SaveChangesAsync();
            }
            return Redirect("~/Contests/Details?id=" + ContestId);
        }

        // POST: Contests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,OwnerEmail,Title,Description,IsNsfw,IsDeleted,IsBanned,SkillLevel,MaxParticipants,CurrentParticipants,FirstPlaceUserEmail,SecondPlaceUserEmail,ThirdPlaceUserEmail,Created,Deadline,Branch")] Contest contest)
        {
            contest.OwnerEmail = User.Identity.Name;

            if(contest.Title == "" || contest.Title == null)
            {
                contest.Title = "Empty";
            }

            if (contest.Description == "" || contest.Description == null)
            {
                contest.Description = "Empty";
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
                    contest.SkillLevel = "All";
                    break;
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
                    contest.MaxParticipants = 10;
                    break;
            }

            if (contest.Deadline == null || contest.Deadline <= DateTime.Now)
            {
                contest.Deadline = null;
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
                    contest.Branch = "Other";
                    break;
            }

            contest.ThirdPlaceUserEmail = null;
            contest.SecondPlaceUserEmail = null;
            contest.FirstPlaceUserEmail = null;
            contest.CurrentParticipants = 0;
            contest.Created = DateTime.Now;

            contest.IsBanned = false;
            contest.IsDeleted = false;
            if ( true || ModelState.IsValid)
            {
                _context.Contests.Add(contest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyContests));
            }
            return View(contest);
        }

        // GET: Contests/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            return NotFound();
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
            return NotFound();
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

        [Authorize]
        public async Task<IActionResult> Join(int id)
        {
            if (id == null || _context.Contests == null)
            {
                return NotFound();
            }

            if (_context.Contests == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contests'  is null.");
            }
            var contest = await _context.Contests.FindAsync(id);
            if (contest != null && contest.IsDeleted == false && contest.IsBanned == false)
            {
                var joinRecord = _context.ContestParticipants.FirstOrDefault(m => m.ContestId == contest.Id);
                if (joinRecord == null)
                {
                    if(contest.MaxParticipants <= contest.CurrentParticipants || (contest.Deadline != null && contest.Deadline <= DateTime.Now))
                    {
                        return Redirect("~/Contests/Details?id=" + id);
                    }
                    try
                    {
                        _context.ContestParticipants.Add(new ContestParticipant
                        {
                            ContestId = id,
                            ParticipantEmail = User.Identity.Name
                        });
                        contest.CurrentParticipants++;
                        _context.Contests.Update(contest);
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
                }

            }

            return Redirect("~/Contests/Details?id=" + id);


        }

        [Authorize]
        public async Task<IActionResult> Leave(int id)
        {
            if (id == null || _context.ContestParticipants == null)
            {
                return NotFound();
            }

            if (_context.ContestParticipants == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contests'  is null.");
            }
            var contest = _context.Contests.FirstOrDefault(p => p.Id == id);
            var contestParticipant = _context.ContestParticipants
                .FirstOrDefault(p => p.ContestId == id && p.ParticipantEmail == User.Identity.Name);


            bool userAlreadySubmittedArt = false;
            if(_context.ContestSubmissions != null)
            {
                var contestUserSubmission = _context.ContestSubmissions
                .FirstOrDefault(p => p.ContestId == id && p.Username == User.Identity.Name);
                if (contestUserSubmission == null)
                {
                    userAlreadySubmittedArt = false;
                }
                else
                {
                    userAlreadySubmittedArt = true;
                }
            }
            else
            {
                userAlreadySubmittedArt = false;
            }


            if (!userAlreadySubmittedArt && contestParticipant != null && contest != null && contest.Deadline > DateTime.Now)
            {
                try
                {
                    _context.ContestParticipants.Remove(contestParticipant);
                    contest.CurrentParticipants--;
                    _context.Contests.Update(contest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContestExists(contestParticipant.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

            }

            if(userAlreadySubmittedArt)
            {
                return Redirect("~/Contests/Details?id=" + id + "&alreadySubmitted=1");
            }
            else
            {
                return Redirect("~/Contests/Details?id=" + id);
            }
            


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
                        _context.Contests.Update(contest);
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
