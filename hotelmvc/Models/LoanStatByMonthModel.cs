namespace hotelmvc.Models
{
    public class LoanStatByMonthModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int LoanCount { get; set; }
        public decimal TotalLoanAmount { get; set; }

    }
}
