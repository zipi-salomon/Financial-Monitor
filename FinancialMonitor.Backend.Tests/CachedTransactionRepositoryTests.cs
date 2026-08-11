using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FinancialMonitor.Backend.Models;
using FinancialMonitor.Backend.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace FinancialMonitor.Backend.Tests
{
    public class CachedTransactionRepositoryTests
    {
        private readonly Mock<ITransactionRepository> _mockInnerRepo;
        private readonly Mock<IConnectionMultiplexer> _mockRedis;
        private readonly Mock<IDatabase> _mockRedisDb;
        private readonly Mock<ILogger<CachedTransactionRepository>> _mockLogger;

        public CachedTransactionRepositoryTests()
        {
            _mockInnerRepo = new Mock<ITransactionRepository>();
            _mockRedis = new Mock<IConnectionMultiplexer>();
            _mockRedisDb = new Mock<IDatabase>();
            _mockLogger = new Mock<ILogger<CachedTransactionRepository>>();

            _mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockRedisDb.Object);
        }

        private CachedTransactionRepository CreateRepository()
        {
            return new CachedTransactionRepository(
                _mockInnerRepo.Object, 
                _mockRedis.Object, 
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task AddAsync_ShouldAddToInnerRepoAndInvalidateCache()
        {
            // Arrange
            var repository = CreateRepository();
            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                Amount = 100.50m,
                Currency = "USD",
                Status = "Completed",
                Timestamp = DateTime.UtcNow
            };

            // Act
            await repository.AddAsync(transaction);

            // Assert
            _mockInnerRepo.Verify(r => r.AddAsync(It.Is<Transaction>(t => t.TransactionId == transaction.TransactionId)), Times.Once);
            _mockRedisDb.Verify(db => db.KeyDeleteAsync("recent_transactions", It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnTransactionsFromRedis_WhenCacheHit()
        {
            // Arrange
            var existingTxList = new List<Transaction>
            {
                new Transaction
                {
                    TransactionId = "TX-REDIS-123",
                    Amount = 250m,
                    Currency = "EUR",
                    Status = "Completed",
                    Timestamp = DateTime.UtcNow
                }
            };

            var jsonString = JsonSerializer.Serialize(existingTxList);

            _mockRedisDb.Setup(db => db.StringGetAsync("recent_transactions", It.IsAny<CommandFlags>()))
                        .ReturnsAsync(jsonString);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().ContainSingle();
            result.First().TransactionId.Should().Be("TX-REDIS-123");
            _mockInnerRepo.Verify(r => r.GetAllAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFetchFromDbAndPopulateRedis_WhenCacheMiss()
        {
            // Arrange
            var dbTransactions = new List<Transaction>
            {
                new Transaction
                {
                    TransactionId = "TX-DB-999",
                    Amount = 500m,
                    Currency = "USD",
                    Status = "Completed",
                    Timestamp = DateTime.UtcNow
                }
            };

            _mockRedisDb.Setup(db => db.StringGetAsync("recent_transactions", It.IsAny<CommandFlags>()))
                        .ReturnsAsync(RedisValue.Null);

            _mockInnerRepo.Setup(r => r.GetAllAsync(It.IsAny<int>())).ReturnsAsync(dbTransactions);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().ContainSingle();
            result.First().TransactionId.Should().Be("TX-DB-999");
            _mockInnerRepo.Verify(r => r.GetAllAsync(It.IsAny<int>()), Times.Once);
            _mockRedisDb.Verify(db => db.StringSetAsync(
                "recent_transactions",
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
            ), Times.Once);
        }
    }
}