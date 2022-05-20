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
    public class AboutMeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AboutMeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: AboutMe
        public async Task<IActionResult> Index(string? id)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }
            
            if (id == null || id == "my")
            {
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    id = _userManager.GetUserId(User);
                }
                else
                {
                    return Redirect("SearchForUsers");
                }
            }

            var userAboutMeData = _context.AboutMe.FirstOrDefault(p => p.UserIdentity == id);
            if (userAboutMeData == null)
            {
                // Testing if user exist
                var userLoginData = await _userManager.FindByIdAsync(id);
                if(userLoginData == null)
                {
                    _context.AboutMe.Add(new AboutMe()
                    {
                        UserIdentity = id,
                        Fullname = "",
                        Caption = "",
                        Bio = ""
                    });
                    _context.SaveChanges();
                }
            }
            var userAboutMeDataResult = await _context.AboutMe.Where(p => p.UserIdentity == id).ToListAsync();

            if (userAboutMeDataResult.Count > 0) return View(userAboutMeDataResult);
            else return Redirect("SearchForUser?notFound=true");


        }

        // GET: AboutMe/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            return NotFound();
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = ViewData["UserIdentity"] = "User not loged in";
            }
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

        public async Task<IActionResult> SearchForUser(string? notFound)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }
            if(notFound == "true")
            {
                ViewData["UserNotFound"] = "true";
            }
            else
            {
                ViewData["UserNotFound"] = "false";
            }
            return View();
        }

        public async Task<IActionResult> SearchForUserConfirm(string UserIdentity)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }
            if (_context.AboutMe == null)
            {
                return Redirect("~Home/Index");
            }

            /// TU NIE DZIAŁA
            var aboutMe = _context.AboutMe.FirstOrDefault(p => p.UserIdentity == UserIdentity);
            string aboutMeResultId = "NotFound";
            if(aboutMe == null)
            {
                var userForUserManager = _userManager.Users.FirstOrDefault(p => p.Email == UserIdentity);
                if(userForUserManager != null)
                {
                    aboutMe = _context.AboutMe
                        .FirstOrDefault(p => p.UserIdentity == userForUserManager.Id);
                    aboutMeResultId = aboutMe.UserIdentity;
                }
            }
            else
            {
                aboutMeResultId = aboutMe.UserIdentity;
            }
            // DOTONT


            if (aboutMe == null)
            {
                return Redirect("SearchForUser?notFound=true");
            }
            return Redirect("Index?id=" + aboutMeResultId);
        }

        // GET: AboutMe/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = ViewData["UserIdentity"] = "User not loged in";
            }
            if (_context.AboutMe == null)
            {
                return Redirect("~AboutMe/Index");
            }

            var aboutMe = _context.AboutMe.FirstOrDefault(p => p.UserIdentity == ViewData["UserIdentity"].ToString());
            if (aboutMe == null)
            {
                return Redirect("~AboutMe/Index");
            }
            return View(aboutMe);
        }

        // POST: AboutMe/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserIdentity,Fullname,Caption,Bio")] AboutMe aboutMe)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserIdentity"] = _userManager.GetUserId(User);
            }
            else
            {
                ViewData["UserIdentity"] = "User not loged in";
            }
            if (id != aboutMe.Id)
            {
                return NotFound();
            }

            aboutMe.UserIdentity = ViewData["UserIdentity"].ToString();


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

        private bool AboutMeExists(int id)
        {
          return (_context.AboutMe?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
