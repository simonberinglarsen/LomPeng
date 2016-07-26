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
    public interface IPermissionService
    {
        bool UserIsAccountAdmin(ClaimsPrincipal user, int childAccountId);
    }
}
