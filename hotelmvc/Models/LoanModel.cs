using System;
using System.ComponentModel.DataAnnotations;

namespace hotelmvc.Models
{
    public class LoanModel
    {
        public int LoanId { get; set; }

        public int CustomerId { get; set; }
        public CustomerModel? Customer { get; set; }


        public decimal LoanAmount { get; set; }


        public DateTime StartDate { get; set; }


        public DateTime EndDate { get; set; }

        public string Status { get; set; } // Pending, Approved, Rejected, Paid
    }
}