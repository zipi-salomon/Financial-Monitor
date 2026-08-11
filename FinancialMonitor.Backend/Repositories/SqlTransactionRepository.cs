using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialMonitor.Backend.Data;
using FinancialMonitor.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialMonitor.Backend.Repositories
{
    public class SqlTransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public SqlTransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync(int count = 20)
        {
            return await _context.Transactions
                .OrderByDescending(t => t.Timestamp)
                .Take(count)
                .ToListAsync(); 
        }

        public async Task AddAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }
    }
}