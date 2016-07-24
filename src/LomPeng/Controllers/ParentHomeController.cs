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


namespace LomPeng.Controllers
{
    [Authorize]
    public class ParentHomeController : Controller
    {
        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private string _userId;
        public ParentHomeController(
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
            ParentHomeViewModel vm = new ParentHomeViewModel();
            using (_context)
            {
                var childAccounts = _context.ChildAccounts
                    .Include(c => c.Child)
                    .Where(acc => acc.ChildAccountAdministrators.Any(adm => adm.ApplicationUser.Id == _userId)).OrderBy(o => o.Child.DisplayName)
                    .Select(acc => new { Account = acc, TotalAmount = acc.Transcations.Sum(t => t.Amount) })
                    .ToList();
                vm.Accounts = new List<ChildAccountViewModel>();
                foreach (var acc in childAccounts)
                {
                    vm.Accounts.Add(new ChildAccountViewModel()
                    {
                        Id = acc.Account.Id,
                        Name = acc.Account.Child.DisplayName,
                        AccountTotal = acc.TotalAmount
                    });
                }

            }

            return View(vm);
        }

        public IActionResult ManageAccount(int id)
        {

            using (_context)
            {
                var childAccount = _context.ChildAccounts
                    .Include(c => c.Child)
                    .Include(c => c.Transcations)
                    .Where(acc => acc.Id == id).Single();
                ManageAccountViewModel vm = new ManageAccountViewModel()
                {
                    Id = childAccount.Id,
                    AccountTotal = childAccount.Transcations.Sum(t => t.Amount),
                    Name = childAccount.Child.DisplayName,
                    NewTransactionAmount = 100,
                    NewTransactionDescription = "Legetøj",
                };
                vm.Transactions = new List<TransactionViewModel>();
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

        [HttpPost]
        public IActionResult NewTransaction(ManageAccountViewModel vm, string button)
        {
            vm.NewTransactionAmount = Math.Abs(vm.NewTransactionAmount);
            if (button == "Withdraw")
            {
                vm.NewTransactionAmount = -vm.NewTransactionAmount;
            }
            using (_context)
            {
                ChildAccount fromAccount = new ChildAccount() { Id = vm.Id };
                _context.Entry(fromAccount).State = EntityState.Unchanged;
                var newTransaction = new Transcation()
                {
                    Amount = vm.NewTransactionAmount,
                    Description = vm.NewTransactionDescription,
                    FromAccount = fromAccount,
                    TimeStamp = DateTime.Now
                };
                _context.Transactions.Add(newTransaction);

                _context.SaveChanges();
            }

            return RedirectToAction("ManageAccount", new { id = vm.Id });
        }

        public IActionResult NewAccount()
        {
            using (_context)
            {
                var currentUser = _context.Users.Where(u => u.Id == _userId).Single();

                // create unknown user and get unique number for dummy user
                var unkownUser = new UnknownUser();
                _context.UnknownUsers.Add(unkownUser);
                _context.SaveChanges();

                // create child user
                var name = "navn_" + unkownUser.Id;
                var email = name + "@lompeng.cu.cc";
                var childUser = new ApplicationUser { DisplayName = name, UserName = email, Email = email };
                var result = _userManager.CreateAsync(childUser, "1q2w3e¤R").Result;
                result = _userManager.AddToRoleAsync(childUser, "Child").Result;

                // create child account
                var childAccount = new ChildAccount()
                {
                    Child = childUser,
                    Transcations = null
                };
                _context.ChildAccounts.Add(childAccount);

                // create admins for child account
                var childAccountAdmin = new ChildAccountAdministrator()
                {
                    ApplicationUser = currentUser,
                    ChildAccount = childAccount
                };
                _context.ChildAccountAdministrators.Add(childAccountAdmin);

                // save all
                _context.SaveChanges();
            }
            return RedirectToAction("");
        }

        public IActionResult EditAccount(int id)
        {
            using (_context)
            {
                var childAccount = _context.ChildAccounts
                    .Include(c => c.Child)
                    .Where(acc => acc.Id == id)
                    .Select(acc => new { Account = acc, TotalAmount = acc.Transcations.Sum(t => t.Amount) })
                    .Single();
                EditAccountViewModel vm = new EditAccountViewModel()
                {
                    Id = childAccount.Account.Id,
                    DisplayName = childAccount.Account.Child.DisplayName,
                    UserName = childAccount.Account.Child.UserName,
                    Password = "nyt password?",
                    AccountTotal = childAccount.TotalAmount,
                };
                return View(vm);
            }
        }

        [HttpPost]
        public IActionResult EditAccount(EditAccountViewModel vm)
        {
            using (_context)
            {
                var childAccount = _context.ChildAccounts
                    .Include(c => c.Child)
                    .Where(acc => acc.Id == vm.Id)
                    .Single();
                var childUser = childAccount.Child;
                childUser.DisplayName = vm.DisplayName;
                childUser.Email = vm.UserName;
                childUser.NormalizedEmail = vm.UserName.ToUpper();
                childUser.UserName = vm.UserName;
                childUser.NormalizedUserName = vm.UserName.ToUpper();
                _context.SaveChanges();
                var hasher = new PasswordHasher<ApplicationUser>();
                var hash = hasher.HashPassword(childUser, vm.Password);
                childUser.PasswordHash = hash;
                _context.SaveChanges();
                return RedirectToAction("ManageAccount", "ParentHome", new { Id = vm.Id });
            }

        }

        public IActionResult DeleteAccount(int id)
        {
            using (_context)
            {
                var childAccount = _context.ChildAccounts
                    .Include(x => x.Child)
                    .Include(x => x.ChildAccountAdministrators)
                    .Include(x => x.Transcations)
                    .Where(acc => acc.Id == id).Single();
                _context.Users.Remove(childAccount.Child);
                _context.ChildAccountAdministrators.RemoveRange(childAccount.ChildAccountAdministrators);
                _context.ChildAccounts.Remove(childAccount);
                _context.Transactions.RemoveRange(childAccount.Transcations);
                _context.SaveChanges();
            }
            return RedirectToAction("");
        }


    }


}
