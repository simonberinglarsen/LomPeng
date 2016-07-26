using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LomPeng.Models.ParentHomeViewModels
{
    public class EditAccountViewModel
    {
        public int Id { get; set; }
        public string NewDisplayName { get; set; }
        public string NewUserName { get; set; }
        public string OldDisplayName { get; set; }
        public string OldUserName { get; set; }
        public double AccountTotal { get; set; }
        public string Password { get; set; }
        public string NewAdminUserName { get; set; }
        public List<string> Administrators { get; set; }
        public string CurrentUser { get; set; }
    }

}
