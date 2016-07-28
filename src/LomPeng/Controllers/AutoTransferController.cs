using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LomPeng.Data;
using LomPeng.Models;
using Microsoft.AspNetCore.Identity;

namespace LomPeng.Controllers
{
    public class AutoTransferController : Controller
    {
        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AutoTransferController(
                ApplicationDbContext context,
                UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            // Insert auto-transfer logic here
            return RedirectToAction("", "Home");
        }
    }
}