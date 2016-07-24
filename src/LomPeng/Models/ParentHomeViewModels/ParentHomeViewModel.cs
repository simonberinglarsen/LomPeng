using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LomPeng.Models.ParentHomeViewModels
{
    public class ParentHomeViewModel
    {   
        public List<ChildAccountViewModel> Accounts { get; set; }
    }

    public class ChildAccountViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double AccountTotal { get; set; }
    }
}
