
using System.Text.Json.Serialization;

    namespace hotelmvc.Models
{
   

    public class LoanStatModel
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("loanCount")]
        public int LoanCount { get; set; }

        [JsonPropertyName("totalLoanAmount")]
        public decimal TotalLoanAmount { get; set; }
    }
}
