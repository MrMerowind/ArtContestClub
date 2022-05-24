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

            
            var userFriend = await _context.Friends
                .FirstOrDefaultAsync(p => p.UserIdentity == _userManager.GetUserId(User) && p.FriendIdentity == id);
            if (userFriend == null)
            {
                ViewData["IsFriend"] = "false";
            }
            else
            {
                ViewData["IsFriend"] = "true";
            }
            var userAboutMeDataResult = await _context.AboutMe.Where(p => p.UserIdentity == id).ToListAsync();

            if(userAboutMeDataResult != null)
            foreach(var a in userAboutMeDataResult)
                ViewData[a.UserIdentity.ToString()] = GetUsernameOrEmailFromUserIdentity(a.UserIdentity.ToString());

            if (userAboutMeDataResult != null && userAboutMeDataResult.Count > 0) return View(userAboutMeDataResult);
            else return Redirect("SearchForUser?notFound=true");


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

            if (UserIdentity.Length > 50)  UserIdentity = UserIdentity.Substring(0, 50);

            /// TU NIE DZIAŁA
            var aboutMe = _context.AboutMe.FirstOrDefault(p => p.UserIdentity == UserIdentity);
            string aboutMeResultId = "NotFound";
            if(aboutMe == null)
            {
                var userForUserManager = _userManager.Users.FirstOrDefault(p => p.Email == UserIdentity);
                if(userForUserManager != null)
                {
                    aboutMe = await _context.AboutMe
                        .FirstOrDefaultAsync(p => p.UserIdentity == userForUserManager.Id);
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

            var aboutMe = await _context.AboutMe.FirstOrDefaultAsync(p => p.UserIdentity == ViewData["UserIdentity"].ToString());
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

            if (aboutMe.Fullname == null) aboutMe.Fullname = "";
            if (aboutMe.Caption == null) aboutMe.Caption = "";
            if (aboutMe.Bio == null) aboutMe.Bio = "";

            if (aboutMe.Fullname.Length > 50) aboutMe.Fullname = aboutMe.Fullname.Substring(0, 50);
            if (aboutMe.Caption.Length > 50) aboutMe.Caption = aboutMe.Caption.Substring(0, 50);
            if (aboutMe.Bio.Length > 500) aboutMe.Bio = aboutMe.Bio.Substring(0, 500);

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
