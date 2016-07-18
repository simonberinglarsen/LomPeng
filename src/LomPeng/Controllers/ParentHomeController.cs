using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LomPeng.Controllers
{
    public class ParentHomeController : Controller
    {
        public IActionResult Index()
        {
           
            return View();
        }

        public IActionResult ManageAccount(int? id)
        {
            return View();
        }

        public IActionResult NewTransaction(int? id, double Amount)
        {
            return RedirectToAction("ManageAccount", new { id = id });
        }

        public IActionResult NewAccount()
        {
            return RedirectToAction("");
        }


    }
}
