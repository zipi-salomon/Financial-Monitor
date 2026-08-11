using System.Text.Json.Serialization;

namespace FinancialMonitor.Backend.Models
{
    public class Transaction
    {
        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "USD";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "Pending"; 

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}