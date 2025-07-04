using System;
using System.ComponentModel.DataAnnotations;

namespace hotelmvc.Models
{
    public class TransactionModel
    {
        public int TransactionId { get; set; }

        public int AccountId { get; set; }

        public AccountModel? Account { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}