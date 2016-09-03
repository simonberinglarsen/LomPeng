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
using LomPeng.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LomPeng.Controllers
{
    [Authorize(Roles = "Parent")]
    public class ParentHomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPermissionService _permissionService;

        public ParentHomeController(
                ApplicationDbContext context,
                UserManager<ApplicationUser> userManager,
                IPermissionService permissionService)
        {
            _userManager = userManager;
            _context = context;
            _permissionService = permissionService;
        }

        public IActionResult Index()
        {
            var childAccounts = _context.ChildAccounts
                .Include(c => c.Child)
                .Where(acc => acc.ChildAccountAdministrators.Any(adm => adm.ApplicationUser.UserName == User.Identity.Name)).OrderBy(o => o.Child.DisplayName)
                .Select(acc => new { Account = acc, TotalAmount = acc.Transcations.Sum(t => t.Amount) })
                .ToList();
            ParentHomeViewModel vm = new ParentHomeViewModel();


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


            return View(vm);
        }

        public IActionResult ManageAccount(int id)
        {
            if (!_permissionService.UserIsAccountAdmin(User, id))
                return RedirectToAction("");
            var vm = LoadManageAccountViewModel(id);
           
            return View(vm);

        }
        private ManageAccountViewModel LoadManageAccountViewModel(int id)
        {
            var childAccount = _context.ChildAccounts
                .Include(c => c.Child)
                .Include(c => c.Transcations).Single(acc => acc.Id == id);
            ManageAccountViewModel vm = new ManageAccountViewModel()
            {
                Id = childAccount.Id,
                AccountTotal = childAccount.Transcations.Sum(t => t.Amount),
                Name = childAccount.Child.DisplayName,
                NewTransactionAmount = null,
                NewTransactionDescription = null,
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
            return vm;
        }
        [HttpPost]
        public IActionResult NewTransaction(ManageAccountViewModel vm, string button)
        {
            if (!_permissionService.UserIsAccountAdmin(User, vm.Id))
                return RedirectToAction("");

            if (!vm.NewTransactionAmount.HasValue)
            {
                vm = LoadManageAccountViewModel(vm.Id);
                return View("ManageAccount", vm);
            }

            vm.NewTransactionAmount = Math.Abs(vm.NewTransactionAmount.Value);
            if (button == "Withdraw")
            {
                vm.NewTransactionAmount = -vm.NewTransactionAmount;
            }
            ChildAccount fromAccount = new ChildAccount() { Id = vm.Id };
            _context.Entry(fromAccount).State = EntityState.Unchanged;
            var newTransaction = new Transcation()
            {
                Amount = vm.NewTransactionAmount.Value,
                Description = vm.NewTransactionDescription,
                FromAccount = fromAccount,
                TimeStamp = DateTime.Now
            };
            _context.Transactions.Add(newTransaction);
            _context.SaveChanges();

            return RedirectToAction("ManageAccount", new { id = vm.Id });
        }

        public IActionResult NewAccount()
        {
            var currentUser = _context.Users.Single(u => u.UserName == User.Identity.Name);

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
                AutoTransfer = new AutoTransferSettings() { AutoTransferIntervalInMinutes = 0, LastUpdate = null },
                Transcations = null
            };

            _context.AutoTransferSettings.Add(childAccount.AutoTransfer);
            _context.SaveChanges();
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

            return RedirectToAction("");
        }

        public IActionResult EditAccount(int id)
        {
            if (!_permissionService.UserIsAccountAdmin(User, id))
                return RedirectToAction("");

            var childAccount = _context.ChildAccounts
                .Include(c => c.Child)
                .Include(c => c.ChildAccountAdministrators).ThenInclude(adm => adm.ApplicationUser)
                .Include(c => c.AutoTransfer)
                .Where(acc => acc.Id == id)
                .Select(acc => new { Account = acc, TotalAmount = acc.Transcations.Sum(t => t.Amount) })
                .Single();
            EditAccountViewModel vm = new EditAccountViewModel()
            {
                Id = childAccount.Account.Id,
                OldDisplayName = childAccount.Account.Child.DisplayName,
                OldUserName = childAccount.Account.Child.UserName,
                NewDisplayName = "",
                NewUserName = "",
                Password = "",
                AccountTotal = childAccount.TotalAmount,
                CurrentUser = User.Identity.Name,
                Administrators = childAccount.Account.ChildAccountAdministrators.Select(adm => adm.ApplicationUser.UserName).ToList(),

                AutoTransferAmount = childAccount.Account.AutoTransfer.AutoTransferAmount,
                AutoTransferFirstPayment = childAccount.Account.AutoTransfer.AutoTransferFirstPayment,
                AutoTransferIntervalInHours = childAccount.Account.AutoTransfer.AutoTransferIntervalInMinutes / 60.0,
                AutoTransferDescription = childAccount.Account.AutoTransfer.AutoTransferDescription,
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult EditAccount(EditAccountViewModel vm)
        {
            if (!_permissionService.UserIsAccountAdmin(User, vm.Id))
                return RedirectToAction("");

            var childAccount = _context.ChildAccounts
                .Include(c => c.Child)
                .Single(acc => acc.Id == vm.Id);
            var childUser = childAccount.Child;
            if (!string.IsNullOrWhiteSpace(vm.NewDisplayName))
                childUser.DisplayName = vm.NewDisplayName;
            if (!string.IsNullOrWhiteSpace(vm.NewUserName))
            {
                childUser.Email = vm.NewUserName;
                childUser.NormalizedEmail = vm.NewUserName.ToUpper();
                childUser.UserName = vm.NewUserName;
                childUser.NormalizedUserName = vm.NewUserName.ToUpper();
            }
            _context.SaveChanges();
            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                var hasher = new PasswordHasher<ApplicationUser>();
                var hash = hasher.HashPassword(childUser, vm.Password);
                childUser.PasswordHash = hash;
                _context.SaveChanges();
            }
            return RedirectToAction("EditAccount", "ParentHome", new { Id = vm.Id });

        }

        [HttpPost]
        public IActionResult AddAdministrator(EditAccountViewModel vm)
        {
            if (!_permissionService.UserIsAccountAdmin(User, vm.Id))
                return RedirectToAction("EditAccount", "ParentHome", new { id = vm.Id });

            // find user (new admin)
            var newAdminUser = _context.Users.Where(u => u.UserName == vm.NewAdminUserName).SingleOrDefault();
            if (newAdminUser == null)
                return RedirectToAction("EditAccount", "ParentHome", new { id = vm.Id });
            if (newAdminUser.UserName == User.Identity.Name)
                return RedirectToAction("EditAccount", "ParentHome", new { id = vm.Id });

            // add user/admin to childaccount
            ChildAccount childAccount = new ChildAccount() { Id = vm.Id };
            _context.Entry(childAccount).State = EntityState.Unchanged;
            var newAccountAdmin = new ChildAccountAdministrator()
            {
                ApplicationUser = newAdminUser,
                ChildAccount = childAccount,
            };
            _context.ChildAccountAdministrators.Add(newAccountAdmin);
            _context.SaveChanges();


            return RedirectToAction("EditAccount", "ParentHome", new { id = vm.Id });
        }

        public IActionResult RemoveAdministrator(int id, string adminUserName)
        {
            if (!_permissionService.UserIsAccountAdmin(User, id))
                return RedirectToAction("EditAccount", "ParentHome", new { id = id });

            // find adminrelation (admin to be deleted)
            var removeAccountAdmin = _context.ChildAccountAdministrators
                .Include(x => x.ApplicationUser)
                .Where(adm => adm.ApplicationUser.UserName == adminUserName && adm.ChildAccount.Id == id)
                .SingleOrDefault();
            if (removeAccountAdmin == null)
                return RedirectToAction("EditAccount", "ParentHome", new { id = id });
            if (removeAccountAdmin.ApplicationUser.UserName == User.Identity.Name)
                return RedirectToAction("EditAccount", "ParentHome", new { id = id });

            // removeuser/admin to childaccount
            _context.ChildAccountAdministrators.Remove(removeAccountAdmin);
            _context.SaveChanges();

            return RedirectToAction("EditAccount", "ParentHome", new { id = id });
        }

        public IActionResult DeleteAccount(int id)
        {
            if (!_permissionService.UserIsAccountAdmin(User, id))
                return RedirectToAction("");

            var childAccount = _context.ChildAccounts
                .Include(x => x.Child)
                .Include(x => x.ChildAccountAdministrators)
                .Include(x => x.Transcations).Single(acc => acc.Id == id);
            _context.Users.Remove(childAccount.Child);
            _context.ChildAccountAdministrators.RemoveRange(childAccount.ChildAccountAdministrators);
            _context.ChildAccounts.Remove(childAccount);
            _context.Transactions.RemoveRange(childAccount.Transcations);
            _context.SaveChanges();
            return RedirectToAction("");

        }
        public IActionResult AcceptInvite(int id)
        {
            return RedirectToAction("");
        }

        public IActionResult RejectInvite(int id)
        {
            return RedirectToAction("");
        }

        [HttpPost]
        public IActionResult UpdateAutoTransferSettings(EditAccountViewModel vm)
        {
            if (!_permissionService.UserIsAccountAdmin(User, vm.Id))
                return RedirectToAction("");

            var autoTransferSettings = _context.ChildAccounts
                .Include(x => x.AutoTransfer)
                .Where(x => x.Id == vm.Id)
                .Select(x => x.AutoTransfer).Single();
            autoTransferSettings.AutoTransferAmount = vm.AutoTransferAmount;
            autoTransferSettings.AutoTransferFirstPayment = vm.AutoTransferFirstPayment;
            autoTransferSettings.AutoTransferIntervalInMinutes = (int)(vm.AutoTransferIntervalInHours * 60.0);
            autoTransferSettings.AutoTransferDescription = vm.AutoTransferDescription;
            autoTransferSettings.LastUpdate = DateTime.MinValue;
            _context.SaveChanges();

            return RedirectToAction("EditAccount", "ParentHome", new { Id = vm.Id });
        }
    }
}
