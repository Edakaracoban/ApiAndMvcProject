namespace hotelmvc.Models
{
    public class TransactionStatsModel
    {
        public string TransactionType { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public int Count { get; set; }
    }
}
