using System;
using System.ComponentModel.DataAnnotations;
namespace hotelmvc.Models
{
    public class AccountModel
    {
        [Key]
        public int AccountId { get; set; }

        public string AccountNumber { get; set; }


        public decimal Balance { get; set; }


        public int CustomerId { get; set; }

        public CustomerModel? Customer { get; set; }   // Nullable yaptık

        public ICollection<TransactionModel>? Transactions { get; set; }  // Nullable yaptık
    }
}