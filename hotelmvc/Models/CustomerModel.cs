using System;
namespace hotelmvc.Models
{
    public class CustomerModel
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public ICollection<AccountModel>? Accounts { get; set; }
        public ICollection<LoanModel>? Loans { get; set; }
    }
}