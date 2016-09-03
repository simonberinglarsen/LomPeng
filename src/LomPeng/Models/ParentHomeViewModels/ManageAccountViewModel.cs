using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LomPeng.Models.ParentHomeViewModels
{
    public class ManageAccountViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double AccountTotal { get; set; }
        [Required]
        public double? NewTransactionAmount { get; set; }
        public string NewTransactionDescription { get; set; }
        public List<TransactionViewModel> Transactions { get; set; }
    }

    public class TransactionViewModel
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public double AccountTotal { get; set; }
    }
}
