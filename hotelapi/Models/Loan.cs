using System;
using System.ComponentModel.DataAnnotations;

namespace hotelapi.Models
{
    public class Loan
    {
        [Key]
        public int LoanId { get; set; }


        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }


        public decimal LoanAmount { get; set; }


        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }


        public string Status { get; set; } // Pending, Approved, Rejected, Paid



    }
}

