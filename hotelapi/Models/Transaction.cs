using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace hotelapi.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }


        public int AccountId { get; set; }

        public Account? Account { get; set; }


        public string TransactionType { get; set; } // Deposit, Withdraw, Transfer, etc.


        public decimal Amount { get; set; }


        public DateTime Date { get; set; }
    }
}

