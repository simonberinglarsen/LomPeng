using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LomPeng.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // child role..
            if(User.Identity.IsAuthenticated)
            {
                if(User.Identity.Name.Contains("simon"))
                    return RedirectToAction("", "ParentHome");
                return RedirectToAction("", "ChildHome");
            }
            else
            {
                return RedirectToAction("Register", "Account");
            }
            return View();
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
