using System;
namespace hotelmvc.Models
{
	public class HomeIndexViewModel
	{
        public int TotalAccounts { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalBalance { get; set; }
        public int ActiveLoans { get; set; }

        public IEnumerable<LoanModel> Loans { get; set; }
        public IEnumerable<TransactionModel> RecentTransactions { get; set; }
        public IEnumerable<CustomerModel> TopCustomers { get; set; }
        public IEnumerable<AccountModel> RecentlyOpenedAccounts { get; set; }

        public decimal MonthlyRevenue { get; set; }
        public decimal MonthlyExpense { get; set; }


        //public double UsdToTry { get; set; }
        //public double EurToTry { get; set; }

        public int PaidLoanAccount { get; set; }
        public int ApprovedLoanCount { get; set; }
        public int PendingLoanCount { get; set; }
        public int RejectedLoanCount { get; set; }
    }
}

