// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
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
    public class BillsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BillsController> _logger;
        private readonly IValidator _validator;

        public BillsController(ApplicationDbContext context, ILogger<BillsController> logger,
            IValidator validator)
        {
            _context = context;
            _logger = logger;
            _validator = validator;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<BillViewModel>>> GetPayments(Guid accountId)
        {
            // Get all payments
            var payments = await _context.Payments
                .Where(x => x.AccountId == accountId)
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (payments == null || !payments.Any())
            {
                return new List<BillViewModel>();
            }

            // Convert to view models
            var viewModels = payments
                .Select(x => new BillViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    DueDate = x.DueDate,
                    TransactionId = x.TransactionId,
                    AccountId = x.AccountId
                })
                .Distinct()
                .ToList();

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<Payment>> GetPayment(Guid id)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified payment
            var viewModel = await _context.Payments
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .Select(x => new BillViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    DueDate = x.DueDate,
                    TransactionId = x.TransactionId,
                    AccountId = x.AccountId
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown payment {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, viewModel.AccountId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access payment {id}");
                return Forbid();
            }

            // Log the operation
            _logger.LogInformation($"User {userId} read payment {viewModel.Id}");

            return Ok(viewModel);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<BillViewModel>> PutPayment(Guid id, BillViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null || id != viewModel.Id)
            {
                return BadRequest();
            }

            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            var payment = await _context.Payments
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (payment == null)
            {
                _logger.LogWarning($"User id {userId} attempted to update unknown payment {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, payment.Account.Tenant.Id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to update payment {id}");
                return Forbid();
            }

            // Update the payment
            payment.Name = viewModel.Name;
            payment.Amount = viewModel.Amount;
            payment.Currency = viewModel.Currency;
            payment.DueDate = viewModel.DueDate;
            payment.UpdatedUser = userId;
            payment.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(payment);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Log the operation
            _logger.LogInformation($"User {userId} updated payment {payment.Id}");

            return Ok(new BillViewModel
            {
                Id = payment.Id,
                Name = payment.Name,
                Amount = payment.Amount,
                Currency = payment.Currency,
                DueDate = payment.DueDate,
                TransactionId = payment.TransactionId,
                AccountId = payment.AccountId
            });
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<BillViewModel>> PostPayment(BillViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null)
            {
                return BadRequest();
            }

            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified account
            var account = await _context.Accounts
                .Include(x => x.Tenant)
                .FirstOrDefaultAsync(x => x.Id == viewModel.AccountId)
                .ConfigureAwait(false);
            if (account == null)
            {
                _logger.LogWarning("Could not find account for payment schedule");
                return BadRequest("Invalid account");
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, account.Tenant.Id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to create payments");
                return Forbid();
            }

            // Create the new payment
            var payment = new Payment
            {
                Name = viewModel.Name,
                Amount = viewModel.Amount,
                DueDate = viewModel.DueDate,
                Account = account,
                CreatedUser = userId
            };

            // Optional currency specification
            if (!string.IsNullOrEmpty(viewModel.Currency))
            {
                payment.Currency = viewModel.Currency;
            }

            await _context.Payments.AddAsync(payment)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Log the operation
            _logger.LogInformation($"User {userId} created payment {payment.Id}");

            viewModel.Id = payment.Id;

            return CreatedAtAction("GetPayment", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeletePayment(Guid id)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified payment
            var payment = await _context.Payments
                // Include the account
                .Include(x => x.Account)
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (payment == null)
            {
                _logger.LogWarning($"User id {userId} attempted to delete unknown payment {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, payment.Account.TenantId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to delete payment {id}");
                return Forbid();
            }

            // Disable the payment
            payment.Disabled = true;
            payment.DisabledUser = userId;
            payment.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(payment);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}