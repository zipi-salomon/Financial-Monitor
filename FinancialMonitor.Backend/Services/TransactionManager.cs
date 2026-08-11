using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialMonitor.Backend.DTOs;
using FinancialMonitor.Backend.Hubs;
using FinancialMonitor.Backend.Models;
using FinancialMonitor.Backend.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FinancialMonitor.Backend.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _repository;
        private readonly IHubContext<TransactionHub> _hubContext;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionRepository repository,
            IHubContext<TransactionHub> hubContext,
            ILogger<TransactionService> logger)
        {
            _repository = repository;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<TransactionResponseDto> ProcessTransactionAsync(CreateTransactionDto dto)
        {
            try
            {
                if (dto == null)
                {
                    _logger.LogWarning("ProcessTransactionAsync failed: Payload (dto) is null.");
                    throw new ArgumentNullException(nameof(dto), "Request body cannot be null.");
                }

                if (dto.Amount <= 0)
                {
                    _logger.LogWarning("Invalid transaction attempt: Amount must be positive. Provided: {Amount}", dto.Amount);
                    throw new ArgumentException("Amount must be greater than zero.", nameof(dto.Amount));
                }

                var transaction = new Transaction
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    Amount = dto.Amount,
                    Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "USD" : dto.Currency,
                    Status = string.IsNullOrWhiteSpace(dto.Status) ? "SUCCESS" : dto.Status,
                    Timestamp = DateTime.UtcNow
                };

                await _repository.AddAsync(transaction);
                _logger.LogInformation("Transaction {TransactionId} successfully created and saved.", transaction.TransactionId);

                var responseDto = new TransactionResponseDto
                {
                    TransactionId = transaction.TransactionId,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    Status = transaction.Status,
                    Timestamp = transaction.Timestamp.ToString("o")
                };

                await _hubContext.Clients.All.SendAsync("ReceiveTransaction", responseDto);
                _logger.LogInformation("Transaction {TransactionId} broadcasted via SignalR.", transaction.TransactionId);

                return responseDto;
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not ArgumentNullException)
            {
                _logger.LogError(ex, "An unexpected error occurred while processing transaction.");
                throw;
            }
        }

        public async Task<IEnumerable<TransactionResponseDto>> GetRecentTransactionsAsync(int limit = 50)
        {
            try
            {
                _logger.LogInformation("Fetching recent transactions with limit: {Limit}", limit);

                var transactions = await _repository.GetAllAsync(limit);

                return transactions.Select(t => new TransactionResponseDto
                {
                    TransactionId = t.TransactionId,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    Status = t.Status,
                    Timestamp = t.Timestamp.ToString("o")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve recent transactions.");
                throw;
            }
        }
    }
}