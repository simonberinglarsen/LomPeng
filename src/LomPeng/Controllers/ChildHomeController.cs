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
using LomPeng.Services;

namespace LomPeng.Controllers
{
    [Authorize(Roles = "Child")]
    public class ChildHomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPermissionService _permissionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChildHomeController(
                ApplicationDbContext context,
                IPermissionService permissionService,
                UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _permissionService = permissionService;
            _context = context;
        }

        public IActionResult Index()
        {
            var childAccount = _context.ChildAccounts
                .Include(c => c.Child)
                .Include(c => c.Transcations).Single(acc => acc.Child.UserName == User.Identity.Name);
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
