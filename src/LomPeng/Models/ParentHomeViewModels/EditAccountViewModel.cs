using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LomPeng.Models.ParentHomeViewModels
{
    public class EditAccountViewModel
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public double AccountTotal { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

    }

}
