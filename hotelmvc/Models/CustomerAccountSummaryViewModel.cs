using System;
namespace hotelmvc.Models
{
	public class CustomerAccountSummaryViewModel
	{
        public string CustomerName { get; set; }
        public int AccountCount { get; set; }
        public decimal TotalBalance { get; set; }
    }
}

