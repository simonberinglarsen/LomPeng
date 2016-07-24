using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using LomPeng.Models.ParentHomeViewModels;
using Microsoft.AspNetCore.Authorization;
using LomPeng.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using LomPeng.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LomPeng.Models.ChildHomeViewModels;

namespace LomPeng.Controllers
{
    public class ChildHomeController : Controller
    {
        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private string _userId;

        public ChildHomeController(
                ApplicationDbContext context,
                IHttpContextAccessor httpContextAccessor,
                UserManager<ApplicationUser> userManager
)
        {
            _userManager = userManager;
            _context = context;
            _userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        public IActionResult Index()
        {
            using (_context)
            {
                var childAccount = _context.ChildAccounts
                    .Include(c => c.Child)
                    .Include(c => c.Transcations)
                    .Where(acc => acc.Child.Id == _userId).Single();
                ChildHomeViewModel vm = new ChildHomeViewModel()
                {
                    Name = childAccount.Child.DisplayName,
                    AccountTotal = childAccount.Transcations.Sum(t => t.Amount),
                    Transactions = new List<TransactionViewModel>()
                };
                double accountTotal = 0;
                foreach (var transaction in childAccount.Transcations.OrderBy(o => o.TimeStamp))
                {
                    accountTotal += transaction.Amount;
                    vm.Transactions.Insert(0, new TransactionViewModel
                    {
                        Amount = transaction.Amount,
                        Date = transaction.TimeStamp,
                        Description = transaction.Description,
                        AccountTotal = accountTotal,
                    });

                }

                return View(vm);
            }
        }
        

    }
}
