using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LomPeng.Models
{
    public class ChildAccount
    {
        public int Id { get; set; }
        public ICollection<ChildAccountAdministrator> ChildAccountAdministrators { get; set; }
        public ApplicationUser Child { get; set; }
        public ICollection<Transcation> Transcations { get; set; }
        public AutoTransferSettings AutoTransfer { get; set; }
    }
    public class Transcation
    {
        public int Id { get; set; }
        public ChildAccount FromAccount { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
    }

    public class ChildAccountAdministrator
    {
        public int Id { get; set; }
        public ChildAccount ChildAccount { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
    public class UnknownUser
    {
        public int Id { get; set; }
    }
    public class AutoTransferSettings
    {
        public int Id { get; set; }
        public int AutoTransferIntervalInMinutes { get; set; }
        public DateTime AutoTransferFirstPayment { get; set; }
        public double AutoTransferAmount { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string AutoTransferDescription { get; set; }
    }
}
