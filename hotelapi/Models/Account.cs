using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace hotelapi.Models
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }


        public string AccountNumber { get; set; }


        public decimal Balance { get; set; }

        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }   // Nullable yaptık

        public ICollection<Transaction>? Transactions { get; set; }  // Nullable yaptık
    }
}
