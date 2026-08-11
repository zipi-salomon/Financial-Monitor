using System.Collections.Generic;
using System.Threading.Tasks;
using FinancialMonitor.Backend.Models;

namespace FinancialMonitor.Backend.Repositories
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetAllAsync(int count = 20);
        Task AddAsync(Transaction transaction);
    }
}