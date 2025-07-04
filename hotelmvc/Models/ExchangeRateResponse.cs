using System;
namespace hotelmvc.Models
{
	public class ExchangeRateResponse
	{
        public string Base { get; set; }         // "USD"
        public string Date { get; set; }         // "2025-07-04"
        public Dictionary<string, double> Rates { get; set; } // "TRY": 33.12, etc.
    }
}

