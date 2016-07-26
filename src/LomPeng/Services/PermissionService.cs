using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LomPeng.Data;
using LomPeng.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace LomPeng.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PermissionService(
                ApplicationDbContext context,
                UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        public bool UserIsAccountAdmin(ClaimsPrincipal user, int childAccountId)
        {
            bool userIsAdmin =
                _context.ChildAccounts.Any(
                    acc =>
                        acc.Id == childAccountId &&
                        acc.ChildAccountAdministrators.Any(adm => adm.ApplicationUser.UserName == user.Identity.Name));
            return userIsAdmin;
        }
    }
}
