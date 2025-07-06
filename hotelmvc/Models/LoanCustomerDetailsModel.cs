namespace hotelmvc.Models
{
    public class LoanCustomerDetailsModel
    {
        public int LoanId { get; set; }
        public decimal LoanAmount { get; set; }
        public string Status { get; set; }

        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
    }
}
