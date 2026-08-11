using Microsoft.EntityFrameworkCore;
using FinancialMonitor.Backend.Models;

namespace FinancialMonitor.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
    }
}