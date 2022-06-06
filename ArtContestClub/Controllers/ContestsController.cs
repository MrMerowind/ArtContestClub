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
    public class ContestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ContestsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager; ;
        }

        public string GetUsernameOrEmailFromUserIdentity(string userIdentity)
        {
            if (userIdentity == "Support") return "Support";

            var person = _context.AboutMe.FirstOrDefault(p => p.UserIdentity == userIdentity);
            string result = "Username";
            if(person == null || (person.Fullname == null || person.Fullname == ""))
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
            if (_context.Ranks != null)
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
            else
            {
                return "User";
            }
        }

        public bool IsAbleToJoinContest(string userIdentity)
        {
            var lastContestJoined = _context.ContestParticipants.Where(p => p.UserIdentity == userIdentity).OrderByDescending(p => p.Id).FirstOrDefault();
            if (lastContestJoined == null) return true;
            else
            {
                var thatContest = _context.Contests.Where(p => p.Id == lastContestJoined.ContestId && p.IsDeleted == false && p.IsBanned == false).FirstOrDefault();
                if(thatContest == null)
                {
                    return true;
                }
                else
                {
                    if(thatContest.Deadline > DateTime.Now)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }

        public bool IsAbleToCreateContest(string userIdentity)
        {
            var lastContest = _context.Contests.Where(p => p.UserIdentity == userIdentity).OrderByDescending(p => p.Id).FirstOrDefault();
            if(lastContest == null) return true;
            if(lastContest.Deadline > DateTime.Now)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public int RankToNumber(string rank)
        {
            switch(rank)
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
        public async Task<IActionResult> MyContests(string? userId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = ViewData["UserIdentity"] = "User not loged in";
            }

            int rank = RankToNumber(GetRank(ViewData["UserIdentity"].ToString()));

            if (userId == null || userId == "my") userId = _userManager.GetUserId(User);

            var contests = _context.Contests.Where(p => p.UserIdentity == userId && p.IsDeleted == false).ToList();
            if(rank < 4)
            {
                foreach (var c in contests)
                {
                    c.ContestSubmissions = await _context.ContestSubmissions
                        .Where(p => p.ContestId == c.Id && p.IsDeleted == false && (p.IsBanned == false || (p.IsBanned == true && p.UserIdentity == _userManager.GetUserId(User)))).ToListAsync();
                }
            }
            else
            {
                foreach (var c in contests)
                {
                    c.ContestSubmissions = await _context.ContestSubmissions
                        .Where(p => p.ContestId == c.Id && p.IsDeleted == false).ToListAsync();
                }
            }
            


            if (contests != null)
                foreach (var a in contests)
                {
                    ViewData[a.UserIdentity.ToString()] = GetUsernameOrEmailFromUserIdentity(a.UserIdentity.ToString());
                    if (a.ContestSubmissions != null)
                    foreach(var b in a.ContestSubmissions)
                    {
                            ViewData[b.UserIdentity.ToString()] = GetUsernameOrEmailFromUserIdentity(b.UserIdentity.ToString());
                    }
                }

            return View(contests);
        }

        [Authorize]
        public async Task<IActionResult> JoinedContests(string? userId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = ViewData["UserIdentity"] = "User not loged in";
            }

            if (userId == null || userId == "my") userId = _userManager.GetUserId(User);

            int rank = RankToNumber(GetRank(ViewData["UserIdentity"].ToString()));

            var joinedContests = _context.ContestParticipants
                .Where(p => p.UserIdentity == userId)
                .Select(p => p.ContestId);

            
            var result = await _context.Contests
                .Where(p => joinedContests.Contains(p.Id))
                .Where(p => p.IsDeleted == false && (p.IsBanned == false || (p.IsBanned == true && p.UserIdentity == _userManager.GetUserId(User)) || rank >= 4)).ToListAsync();

            foreach (var c in result)
            {
                c.ContestSubmissions = await _context.ContestSubmissions
                    .Where(p => p.ContestId == c.Id && p.IsDeleted == false && (p.IsBanned == false || (p.IsBanned == true && p.UserIdentity == _userManager.GetUserId(User)) || rank >= 4)).ToListAsync();
            }

            if (result != null)
                foreach (var a in result)
                {
                    ViewData[a.UserIdentity.ToString()] = GetUsernameOrEmailFromUserIdentity(a.UserIdentity.ToString());
                    if (a.ContestSubmissions != null)
                        foreach (var b in a.ContestSubmissions)
                        {
                            ViewData[b.UserIdentity.ToString()] = GetUsernameOrEmailFromUserIdentity(b.UserIdentity.ToString());
                        }
                }
            return View(result);
        }
        public IActionResult SearchContest()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = ViewData["UserIdentity"] = "User not loged in";
            }
            return View();
        }
        public async Task<IActionResult> Index(string? Title, string SkillLevel, string Branch, int page = 0, bool IsNsfw = false)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = ViewData["UserIdentity"] = "User not loged in";
            }

            int rank = RankToNumber(GetRank(ViewData["UserIdentity"].ToString()));


            var searchResult = _context.Contests.AsQueryable();
            if (Title == null) Title = "";
            if (SkillLevel == null || Branch == null) return View();
            if (page <= 0) page = 0;

            if (Title.Length > 100)  Title = Title.Substring(0, 100);

            if (Title != "")
            {
                var arr = Title.Split(' ');

                foreach (var word in arr)
                {
                    searchResult = searchResult.Where(a => a.Title.ToLower().Contains(word.ToLower()));
                }
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


            searchResult = searchResult.Where(p => p.IsDeleted == false);

            searchResult = searchResult.Where(p => p.IsBanned == false|| (p.IsBanned == true && p.UserIdentity == _userManager.GetUserId(User)) || rank >= 4);

            searchResult = searchResult.Skip(20 * page);
            
            var result = await searchResult.Take(20).ToListAsync();

            foreach (var c in result)
            {
                c.ContestSubmissions = await _context.ContestSubmissions
                    .Where(p => p.ContestId == c.Id && p.IsDeleted == false && (p.IsBanned == false || (p.IsBanned == true && p.UserIdentity == _userManager.GetUserId(User)))).ToListAsync();
            }

            if (result != null)
                foreach (var a in result)
                {
                    ViewData[a.UserIdentity.ToString()] = GetUsernameOrEmailFromUserIdentity(a.UserIdentity.ToString());
                    if (a.ContestSubmissions != null)
                        foreach (var b in a.ContestSubmissions)
                        {
                            ViewData[b.UserIdentity.ToString()] = GetUsernameOrEmailFromUserIdentity(b.UserIdentity.ToString());
                        }
                }


            return View(result);

        }

        [Authorize]
        public async Task<IActionResult> SetPlace(int? id, int? place, string? placeUserIdentity)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            if (id == null || _context.Contests == null || place == null || placeUserIdentity == null)
            {
                return NotFound();
            }

            var contest = await _context.Contests.FirstOrDefaultAsync(m => m.Id == id);
            if (contest == null)
            {
                return NotFound();
            }

            if (placeUserIdentity.Length > 50)  placeUserIdentity = placeUserIdentity.Substring(0, 50);

            if(contest.UserIdentity == _userManager.GetUserId(User))
            {
                switch(place)
                {
                    case 1:
                        contest.FirstPlaceUserEmail = placeUserIdentity;
                        break;
                    case 2:
                        contest.SecondPlaceUserEmail = placeUserIdentity;
                        break;
                    case 3:
                        contest.ThirdPlaceUserEmail = placeUserIdentity;
                        break;
                    default:
                        return NotFound();
                }
            }

            _context.Update(contest);
            await _context.SaveChangesAsync();

            return Redirect("~/Contests/Details?id=" + id);
        }

        // GET: Contests/Details/5
        public async Task<IActionResult> Details(int? id, int alreadySubmitted = 0, int reportContest = 0, bool userRankToLowToJoin = false)
        {
            if(User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }

            int rank = RankToNumber(GetRank(ViewData["UserIdentity"].ToString()));

            if (userRankToLowToJoin)
            {
                ViewData["UserRankToLowToJoin"] = "true";
            }
            else
            {
                ViewData["UserRankToLowToJoin"] = "false";
            }

            if (rank >= 4)
            {
                ViewData["SupportConfirmed"] = "true";
            }
            else
            {
                ViewData["SupportConfirmed"] = "false";
            }

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
                .Where(p => p.UserIdentity == ViewData["UserIdentity"].ToString() && p.ContestId == id).ToListAsync();

            contest.ContestSubmissions = await _context.ContestSubmissions.Where(p => p.ContestId == id && p.IsDeleted == false
            && (p.IsBanned == false || (p.IsBanned == true && p.UserIdentity == _userManager.GetUserId(User)) || rank >= 4)).ToListAsync();


            ViewData[contest.UserIdentity.ToString()] = GetUsernameOrEmailFromUserIdentity(contest.UserIdentity.ToString());

            if (contest.ContestSubmissions != null)
            foreach (var a in contest.ContestSubmissions)
            {
                    ViewData[a.UserIdentity.ToString()] = GetUsernameOrEmailFromUserIdentity(a.UserIdentity.ToString());
            }

            return View(contest);
        }

        // GET: Contests/Create
        [Authorize]
        public IActionResult Create()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = ViewData["UserIdentity"] = "User not loged in";
            }

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 1)
            {
                return RedirectToAction("YouAreBanned", "Ranks");
            }

            return View();
        }

        [Authorize]
        public IActionResult SubmitArt(int id)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = ViewData["UserIdentity"] = "User not loged in";
            }
            return View(new ContestSubmission() { Id = id });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> SubmitArtConfirm(string? Title, string? ArtLink, int? ContestId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = ViewData["UserIdentity"] = "User not loged in";
            }
            if (Title == null || ArtLink == null || ContestId == null) return Redirect("~/Contests/Details?id=" + ContestId);

            if (Title.Length > 100) Title = Title.Substring(0, 100);
            if (ArtLink.Length > 150) ArtLink = ArtLink.Substring(0, 150);

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 1)
            {
                return RedirectToAction("YouAreBanned", "Ranks");
            }

            ContestSubmission contestSubmission = new ContestSubmission()
            {
                Title = Title,
                ArtLink = ArtLink,
                UserIdentity = _userManager.GetUserId(User),
                ContestId = ContestId,
                Submited = DateTime.Now,
                IsDeleted = false,
                IsBanned = false,
            };

            var contest = await _context.Contests.FirstOrDefaultAsync(m => m.Id == ContestId);
            if (contest != null && contest.IsDeleted == false && contest.IsBanned == false &&
                (contest.Deadline == null || (contest.Deadline != null && contest.Deadline > DateTime.Now)) )
            {
                var previousSubmission = _context.ContestSubmissions
                .FirstOrDefault(p => p.ContestId == ContestId && p.UserIdentity == _userManager.GetUserId(User));
                
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
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = ViewData["UserIdentity"] = "User not loged in";
            }

            contest.UserIdentity = ViewData["UserIdentity"].ToString();

            if (contest.Title == "" || contest.Title == null)
            {
                contest.Title = "Empty";
            }

            if (contest.Description == "" || contest.Description == null)
            {
                contest.Description = "Empty";
            }

            int userRank = RankToNumber(GetRank(contest.UserIdentity));
            if (userRank < 1)
            {
                return RedirectToAction("YouAreBanned", "Ranks");
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
                    if (userRank < 3)
                    {
                        ViewData["UserRankToLow"] = "true";
                        return View(contest);
                    }
                    break;
                case 50:
                    if(userRank < 3)
                    {
                        ViewData["UserRankToLow"] = "true";
                        return View(contest);
                    }
                    break;
                case 100:
                    if (userRank < 3)
                    {
                        ViewData["UserRankToLow"] = "true";
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

            if (contest.Title.Length > 100) contest.Title = contest.Title.Substring(0, 100);
            if (contest.Description.Length > 500) contest.Description = contest.Description.Substring(0, 500);

            contest.IsBanned = false;
            contest.IsDeleted = false;

            // If user is premium+ or didnt create then can create contest
            bool isAbleToCreate = userRank >= 3 || IsAbleToCreateContest(_userManager.GetUserId(User));

            if (!isAbleToCreate)
            {
                ViewData["UserRankToLowToCreate"] = "true";
                return View(contest);
            }

            if ( true || ModelState.IsValid)
            {
                _context.Contests.Add(contest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyContests));
            }
            return View(contest);
        }

        [Authorize]
        public async Task<IActionResult> Join(int? id)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = ViewData["UserIdentity"] = "User not loged in";
            }
            if (id == null || _context.Contests == null)
            {
                return NotFound();
            }

            if (RankToNumber(GetRank(ViewData["UserIdentity"].ToString())) < 1)
            {
                return RedirectToAction("YouAreBanned", "Ranks");
            }

            if (_context.Contests == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contests'  is null.");
            }
            var contest = await _context.Contests.FindAsync(id);
            if (contest != null && contest.IsDeleted == false && contest.IsBanned == false)
            {
                var joinRecord = _context.ContestParticipants
                    .FirstOrDefault(m => m.ContestId == contest.Id && m.UserIdentity == _userManager.GetUserId(User));
                if (joinRecord == null)
                {
                    if(contest.MaxParticipants <= contest.CurrentParticipants || (contest.Deadline != null && contest.Deadline <= DateTime.Now))
                    {
                        return Redirect("~/Contests/Details?id=" + id);
                    }

                    int userRank = RankToNumber(GetRank(_userManager.GetUserId(User)));
                    // If user is vip+ or last contest ended then can join
                    bool isAbleToJoin = userRank >= 2 || IsAbleToJoinContest(_userManager.GetUserId(User));

                    if (!isAbleToJoin)
                    {
                        return Redirect("~/Contests/Details?id=" + id + "&userRankToLowToJoin=true");
                    }

                    try
                    {
                        _context.ContestParticipants.Add(new ContestParticipant
                        {
                            ContestId = id,
                            UserIdentity = _userManager.GetUserId(User)
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
        public async Task<IActionResult> Leave(int? id)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = ViewData["UserIdentity"] = "User not loged in";
            }
            if (id == null || _context.ContestParticipants == null)
            {
                return NotFound();
            }

            if (_context.ContestParticipants == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contests'  is null.");
            }
            var contest = _context.Contests.FirstOrDefault(p => p.Id == id);
            var contestParticipant = await _context.ContestParticipants
                .FirstOrDefaultAsync(p => p.ContestId == id && p.UserIdentity == _userManager.GetUserId(User));


            bool userAlreadySubmittedArt = false;
            if(_context.ContestSubmissions != null)
            {
                var contestUserSubmission = _context.ContestSubmissions
                .FirstOrDefault(p => p.ContestId == id && p.UserIdentity == _userManager.GetUserId(User));
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
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = ViewData["UserIdentity"] = "User not loged in";
            }
            if (id == null || _context.Contests == null)
            {
                return NotFound();
            }

            var contest = await _context.Contests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contest == null || contest.UserIdentity != _userManager.GetUserId(User))
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
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = ViewData["UserIdentity"] = "User not loged in";
            }
            if (_context.Contests == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contests'  is null.");
            }
            var contest = await _context.Contests.FindAsync(id);
            if (contest != null)
            {
                if(contest.UserIdentity == _userManager.GetUserId(User))
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
            return RedirectToAction("MyContests");
        }

        private bool ContestExists(int id)
        {
          return (_context.Contests?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
