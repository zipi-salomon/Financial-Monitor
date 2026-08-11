using System.Collections.Generic;
using System.Threading.Tasks;
using FinancialMonitor.Backend.DTOs;

namespace FinancialMonitor.Backend.Services
{
    public interface ITransactionService
    {
        Task<TransactionResponseDto> ProcessTransactionAsync(CreateTransactionDto dto);
        Task<IEnumerable<TransactionResponseDto>> GetRecentTransactionsAsync(int limit = 50);
    }
}