namespace FinancialMonitor.Backend.DTOs
{
    public class CreateTransactionDto
    {
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }
    }
}