using LomPeng.Models.ParentHomeViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LomPeng.Models.ChildHomeViewModels
{
    public class ChildHomeViewModel
    {
        public string Name { get; set; }
        public double AccountTotal { get; set; }
        public List<TransactionViewModel> Transactions { get; set; }
    }
}
