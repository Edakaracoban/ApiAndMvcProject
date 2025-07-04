using System;
namespace hotelmvc.Models
{
	public class AccountSummaryModel
	{
        public string CustomerName { get; set; }
        public int AccountCount { get; set; }
        public decimal TotalBalance { get; set; }
    }
}

