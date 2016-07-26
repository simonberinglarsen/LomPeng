using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LomPeng.Data;
using Microsoft.AspNetCore.Http;
using LomPeng.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace LomPeng.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
                ApplicationDbContext context,
                UserManager<ApplicationUser> userManager
)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            // child role..
                var currentUser = _context.Users.Where(u => u.UserName == User.Identity.Name).SingleOrDefault();
                if(currentUser == null)
                    return RedirectToAction("Register", "Account");
                var isParent = _userManager.IsInRoleAsync(currentUser, "Parent").Result;
                if (isParent)
                {
                    return RedirectToAction("", "ParentHome");
                }
                var isChild = _userManager.IsInRoleAsync(currentUser, "Child").Result;
                if (isChild)
                {
                    return RedirectToAction("", "ChildHome");
                }
                return RedirectToAction("Register", "Account");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
