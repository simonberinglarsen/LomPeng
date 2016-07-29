using System.Linq;
using Microsoft.AspNetCore.Mvc;
using LomPeng.Data;
using LomPeng.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

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

        public JsonResult Run()
        {
            // Insert auto-transfer logic here
            // implement lock(monitor)-mechanism here somehow
            DateTime updateTime = DateTime.Now;
            var childAccounts = _context.ChildAccounts.Include(x => x.AutoTransfer).ToList();
            foreach (var childAccount in childAccounts)
            {
                var autoTransfer = childAccount.AutoTransfer;
                if (autoTransfer == null)
                    continue;
                if (autoTransfer.AutoTransferAmount > 0 && autoTransfer.AutoTransferIntervalInMinutes > 0)
                {
                    // transfer money
                    TimeSpan interval = new TimeSpan(0, autoTransfer.AutoTransferIntervalInMinutes, 0);
                    DateTime nextUpdate;
                    bool neverUpdated = autoTransfer.LastUpdate == null || (autoTransfer.LastUpdate.HasValue && autoTransfer.LastUpdate.Value == DateTime.MinValue);
                    if (neverUpdated)
                        nextUpdate = autoTransfer.AutoTransferFirstPayment;
                    else
                        nextUpdate = autoTransfer.LastUpdate.Value + interval;
                    
                    int maxIterations = 100;
                    while (nextUpdate < updateTime && ((maxIterations--) > 0))
                    {
                        var newTransaction = new Transcation()
                        {
                            Amount = autoTransfer.AutoTransferAmount,
                            Description = autoTransfer.AutoTransferDescription,
                            FromAccount = childAccount,
                            TimeStamp = nextUpdate
                        };
                        _context.Transactions.Add(newTransaction);
                        autoTransfer.LastUpdate = nextUpdate;
                        nextUpdate += interval;
                    }
                }
                
            }
            _context.SaveChanges();
            return Json(new { Done = true });
        }
    }
}