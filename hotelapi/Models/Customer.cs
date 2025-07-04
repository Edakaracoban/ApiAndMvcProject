using System;
using System.ComponentModel.DataAnnotations;

namespace hotelapi.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }


        public string Name { get; set; }


        public string Email { get; set; }


        public string Phone { get; set; }

        public string Address { get; set; }

        public ICollection<Account>? Accounts { get; set; }
        public ICollection<Loan>? Loans { get; set; }
    }
}