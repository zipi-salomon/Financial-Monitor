using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinancialMonitor.Backend.DTOs;
using FinancialMonitor.Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FinancialMonitor.Backend.Controllers
{
    /// <summary>
    /// Handles transaction ingestion and retrieval operations.
    /// Implements distributed locking via Redis for idempotency control.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IDatabase _redisDb;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            ITransactionService transactionService,
            IConnectionMultiplexer redis,
            ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _redisDb = redis.GetDatabase();
            _logger = logger;
        }

        /// <summary>
        /// Processes a new transaction with idempotency protection.
        /// </summary>
        /// <param name="dto">The transaction payload.</param>
        /// <returns>The created transaction response DTO.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TransactionResponseDto>> Create([FromBody] CreateTransactionDto dto)
        {
            // Extract or generate idempotency key from request headers
            if (!Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey) || string.IsNullOrWhiteSpace(idempotencyKey))
            {
                idempotencyKey = Guid.NewGuid().ToString();
            }

            string redisKey = $"idempotency:{idempotencyKey}";

            try
            {
                // Attempt to acquire lock in Redis for 2 minutes using SETNX semantics
                bool isNewRequest = await _redisDb.StringSetAsync(
                    redisKey, 
                    "processing", 
                    TimeSpan.FromMinutes(2), 
                    When.NotExists
                );

                if (!isNewRequest)
                {
                    _logger.LogWarning("Duplicate transaction attempt blocked with key: {Key}", idempotencyKey);
                    return Conflict(new { message = "Transaction is already being processed or was submitted." });
                }

                // Process business logic and save transaction
                var result = await _transactionService.ProcessTransactionAsync(dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // Release Redis lock on validation failures to allow retries
                await _redisDb.KeyDeleteAsync(redisKey);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Release Redis lock on unexpected failure
                await _redisDb.KeyDeleteAsync(redisKey);
                _logger.LogError(ex, "Unhandled exception in Create endpoint.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred." });
            }
        }

        /// <summary>
        /// Retrieves recent transactions up to the specified limit.
        /// </summary>
        /// <param name="limit">Maximum number of items to return (default: 50).</param>
        /// <returns>A collection of recent transaction records.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TransactionResponseDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TransactionResponseDto>>> GetAll([FromQuery] int limit = 50)
        {
            try
            {
                var result = await _transactionService.GetRecentTransactionsAsync(limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in GetAll endpoint.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred." });
            }
        }
    }
}