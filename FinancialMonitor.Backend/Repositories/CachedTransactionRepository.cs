using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FinancialMonitor.Backend.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FinancialMonitor.Backend.Repositories
{
    public class CachedTransactionRepository : ITransactionRepository
    {
        private readonly ITransactionRepository _innerRepository;
        private readonly IDatabase _redisDb;
        private readonly ILogger<CachedTransactionRepository> _logger;
        private const string CacheKey = "recent_transactions";
        private const int DefaultMaxCacheSize = 20;

        public CachedTransactionRepository(
            ITransactionRepository innerRepository, 
            IConnectionMultiplexer redis,
            ILogger<CachedTransactionRepository> logger)
        {
            _innerRepository = innerRepository;
            _redisDb = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync(int count = DefaultMaxCacheSize)
        {
            try
            {
                var redisData = await _redisDb.StringGetAsync(CacheKey);
                if (!redisData.IsNullOrEmpty)
                {
                    var cachedItems = JsonSerializer.Deserialize<IEnumerable<Transaction>>((string)redisData!);
                    if (cachedItems != null)
                    {
                        return cachedItems.Take(count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read transactions from Redis cache. Falling back to database.");
            }

            var transactions = (await _innerRepository.GetAllAsync(count)).ToList();

            try
            {
                var json = JsonSerializer.Serialize(transactions);
                await _redisDb.StringSetAsync(CacheKey, json, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to write transactions to Redis cache.");
            }

            return transactions;
        }

        public async Task AddAsync(Transaction transaction)
        {
            // 1. כתיבה למקור האמת (Database)
            await _innerRepository.AddAsync(transaction);

            // 2. ניקוי ה-Cache
            try
            {
                await _redisDb.KeyDeleteAsync(CacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate Redis cache for key '{CacheKey}' after adding transaction.", CacheKey);
            }
        }
    }
}