// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chabloom.Payments.Data;
using Chabloom.Payments.Models;
using Chabloom.Payments.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Payments.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ApplicationDbContext context, ILogger<TransactionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<TransactionViewModel>>> GetTransactions(Guid? accountId, Guid? tenantId)
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("User attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"User sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // TODO: Role-Based Access

            // Get all transactions the user is authorized to view
            var transactions = await _context.Transactions
                // Include bill account users
                .Include(x => x.Bill)
                .ThenInclude(x => x.Account)
                .ThenInclude(x => x.Users)
                // Include bill tenant users
                .Include(x => x.Bill)
                .ThenInclude(x => x.Account)
                .ThenInclude(x => x.Tenant)
                .ThenInclude(x => x.Users)
                // Ensure the transaction has not been deleted
                .Where(x => !x.Disabled)
                // Ensure the current user exists in bill account or bill tenant users
                .Where(x => x.Bill.Account.Users
                                .Select(y => y.UserId)
                                .Contains(userId) ||
                            x.Bill.Account.Tenant.Users
                                .Select(y => y.UserId)
                                .Contains(userId))
                .ToListAsync()
                .ConfigureAwait(false);
            if (transactions == null)
            {
                return new List<TransactionViewModel>();
            }

            List<TransactionViewModel> viewModels;
            if (accountId != null)
            {
                // Filter transactions by bill account id
                viewModels = transactions
                    .Where(x => x.Bill.Account.Id == accountId)
                    .Select(x => new TransactionViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ExternalId = x.ExternalId,
                        Amount = x.Amount,
                        Bill = x.Bill.Id
                    })
                    .ToList();
            }
            else if (tenantId != null)
            {
                // Filter transactions by bill tenant id
                viewModels = transactions
                    .Where(x => x.Bill.Account.Tenant.Id == tenantId)
                    .Select(x => new TransactionViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ExternalId = x.ExternalId,
                        Amount = x.Amount,
                        Bill = x.Bill.Id
                    })
                    .ToList();
            }
            else
            {
                // Do not filter transactions
                viewModels = transactions
                    .Select(x => new TransactionViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ExternalId = x.ExternalId,
                        Amount = x.Amount,
                        Bill = x.Bill.Id
                    })
                    .ToList();
            }

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TransactionViewModel>> GetTransaction(Guid id)
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("User attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"User sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // TODO: Role-Based Access

            // Find the specified transaction if the user has access to it
            var transaction = await _context.Transactions
                // Include bill account users
                .Include(x => x.Bill)
                .ThenInclude(x => x.Account)
                .ThenInclude(x => x.Users)
                // Include bill tenant users
                .Include(x => x.Bill)
                .ThenInclude(x => x.Account)
                .ThenInclude(x => x.Tenant)
                .ThenInclude(x => x.Users)
                // Ensure the transaction has not been deleted
                .Where(x => !x.Disabled)
                // Ensure the current user exists in bill account or bill tenant users
                .Where(x => x.Bill.Account.Users
                                .Select(y => y.UserId)
                                .Contains(userId) ||
                            x.Bill.Account.Tenant.Users
                                .Select(y => y.UserId)
                                .Contains(userId))
                .Select(x => new TransactionViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ExternalId = x.ExternalId,
                    Amount = x.Amount,
                    Bill = x.Bill.Id
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (transaction == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown transaction {id}");
                return NotFound();
            }

            return Ok(transaction);
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TransactionViewModel>> PostTransaction(
            TransactionViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null)
            {
                return BadRequest();
            }

            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("User attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"User sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // Create the new transaction
            var transaction = new Transaction
            {
                Name = viewModel.Name,
                ExternalId = viewModel.ExternalId,
                Amount = viewModel.Amount,
                Bill = await _context.Bills
                    .Include(x => x.Account)
                    .ThenInclude(x => x.Tenant)
                    .ThenInclude(x => x.Users)
                    .ThenInclude(x => x.Role)
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Bill)
                    .ConfigureAwait(false),
                CreatedUser = userId,
                UpdatedUser = userId,
                DisabledUser = userId
            };

            // Ensure the bill was found
            if (transaction.Bill == null)
            {
                _logger.LogWarning($"Specified bill {viewModel.Bill} could not be found");
                return BadRequest();
            }

            // Ensure the current user belongs to the tenant
            if (!transaction.Bill.Account.Tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {transaction.Bill.Account.Tenant.Id}");
                return Forbid();
            }

            // Ensure the current user is a tenant admin or manager
            var tenantUser = transaction.Bill.Account.Tenant.Users
                .FirstOrDefault(x => x.UserId == userId);
            if (tenantUser != null &&
                tenantUser.Role.Name != "Admin" &&
                tenantUser.Role.Name != "Manager")
            {
                _logger.LogWarning($"User id {userId} did not have permissions to create transaction for tenant {transaction.Bill.Account.Tenant.Id}");
                return Forbid();
            }

            await _context.Transactions.AddAsync(transaction)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = transaction.Id;

            return CreatedAtAction("GetTransaction", new {id = viewModel.Id}, viewModel);
        }
    }
}