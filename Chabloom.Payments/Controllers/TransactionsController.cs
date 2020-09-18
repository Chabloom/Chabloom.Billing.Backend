// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chabloom.Payments.Data;
using Chabloom.Payments.Models;
using Chabloom.Payments.Services;
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
        private readonly IValidator _validator;

        public TransactionsController(ApplicationDbContext context, ILogger<TransactionsController> logger,
            IValidator validator)
        {
            _context = context;
            _logger = logger;
            _validator = validator;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<TransactionViewModel>>> GetTransactions(Guid? accountId,
            Guid? tenantId)
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

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            bool userAuthorized;
            if (accountId != null)
            {
                userAuthorized = await _validator.CheckAccountAccessAsync(userId, accountId.Value)
                    .ConfigureAwait(false);
            }
            else if (tenantId != null)
            {
                userAuthorized = await _validator.CheckTenantAccessAsync(userId, tenantId.Value)
                    .ConfigureAwait(false);
            }
            else
            {
                userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                    .ConfigureAwait(false);
            }

            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access transactions");
                return Forbid();
            }

            // Get all transactions
            var transactions = await _context.Transactions
                // Include the bill, account and tenant
                .Include(x => x.Bill)
                .ThenInclude(x => x.Account)
                .ThenInclude(x => x.Tenant)
                // Ensure the transaction has not been deleted
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (transactions == null)
            {
                return new List<TransactionViewModel>();
            }

            List<TransactionViewModel> viewModels;
            if (accountId != null)
            {
                // Filter transactions by account id
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
                // Filter transactions by tenant id
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

            // Find the specified transaction
            var viewModel = await _context.Transactions
                // Include the bill and account
                .Include(x => x.Bill)
                .ThenInclude(x => x.Account)
                // Ensure the transaction has not been deleted
                .Where(x => !x.Disabled)
                .Select(x => new TransactionViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ExternalId = x.ExternalId,
                    Amount = x.Amount,
                    Bill = x.Bill.Id,
                    Account = x.Bill.Account.Id
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown transaction {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, viewModel.Account)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access transaction {id}");
                return Forbid();
            }

            return Ok(viewModel);
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
                _logger.LogWarning(
                    $"User id {userId} did not have permissions to create transaction for tenant {transaction.Bill.Account.Tenant.Id}");
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