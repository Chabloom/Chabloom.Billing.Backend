// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chabloom.Payments.Data;
using Chabloom.Payments.Models;
using Chabloom.Payments.Services;
using Chabloom.Payments.ViewModels.Payment;
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
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IValidator _validator;

        public PaymentsController(ApplicationDbContext context, ILogger<PaymentsController> logger,
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
        public async Task<ActionResult<IEnumerable<PaymentViewModel>>> GetPayments(Guid? accountId)
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
            else
            {
                userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                    .ConfigureAwait(false);
            }

            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access payments");
                return Forbid();
            }

            // Get all payments
            var payments = await _context.Payments
                // Include the account
                .Include(x => x.Account)
                // Ensure the payment has not been deleted
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (payments == null || !payments.Any())
            {
                return new List<PaymentViewModel>();
            }

            List<PaymentViewModel> viewModels;
            if (accountId != null)
            {
                // Filter payments by account id
                viewModels = payments
                    .Where(x => x.Account.Id == accountId)
                    .Select(x => new PaymentViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Amount = x.Amount,
                        Currency = x.Currency,
                        DueDate = x.DueDate,
                        Complete = x.Complete,
                        Account = x.Account.Id,
                        TransactionSchedule = x.TransactionScheduleId
                    })
                    .Distinct()
                    .ToList();
            }
            else
            {
                // Do not filter payments
                viewModels = payments
                    .Select(x => new PaymentViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Amount = x.Amount,
                        Currency = x.Currency,
                        DueDate = x.DueDate,
                        Complete = x.Complete,
                        Account = x.Account.Id,
                        TransactionSchedule = x.TransactionScheduleId
                    })
                    .Distinct()
                    .ToList();
            }

            return Ok(viewModels);
        }

        [HttpGet("TenantAccount")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<PaymentViewModel>>> GetPayments(string accountNumber, Guid tenantId)
        {
            if (string.IsNullOrEmpty(accountNumber))
            {
                return BadRequest("Tenant and account must both be specified");
            }

            // Get all payments for the specified account number
            var payments = await _context.Payments
                // Include the account and tenant
                .Include(x => x.Account)
                .ThenInclude(x => x.Tenant)
                // Get payments only for the specified tenant
                .Where(x => x.Account.Tenant.Id == tenantId)
                // Get payments only for the specified account number
                .Where(x => x.Account.ExternalId == accountNumber)
                // Ensure the payment has not been deleted
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (payments == null || !payments.Any())
            {
                return new List<PaymentViewModel>();
            }

            var viewModels = payments
                .Select(x => new PaymentViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    DueDate = x.DueDate,
                    Complete = x.Complete,
                    Account = x.Account.Id,
                    TransactionSchedule = x.TransactionScheduleId
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

            // Find the specified payment
            var viewModel = await _context.Payments
                // Include the account
                .Include(x => x.Account)
                // Ensure the payment has not been deleted
                .Where(x => !x.Disabled)
                .Select(x => new PaymentViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    DueDate = x.DueDate,
                    Complete = x.Complete,
                    Account = x.Account.Id,
                    TransactionSchedule = x.TransactionScheduleId
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown payment {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, viewModel.Account)
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
        public async Task<ActionResult<PaymentViewModel>> PutPayment(Guid id, UpdateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null || id != viewModel.Id)
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

            var payment = await _context.Payments
                // Include the account and tenant
                .Include(x => x.Account)
                .ThenInclude(x => x.Tenant)
                // Don't return deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (payment == null)
            {
                _logger.LogWarning($"User id {userId} attempted to update unknown payment {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, payment.Account.Tenant.Id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to create payments");
                return Forbid();
            }

            // Update the payment
            payment.Name = viewModel.Name;
            payment.Amount = viewModel.Amount;
            payment.Currency = viewModel.Currency;
            payment.DueDate = viewModel.DueDate;
            payment.Complete = viewModel.Complete;
            payment.UpdatedUser = userId;
            payment.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(payment);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Log the operation
            _logger.LogInformation($"User {userId} updated payment {payment.Id}");

            // Build the ret view model
            var retViewModel = new PaymentViewModel
            {
                Id = payment.Id,
                Name = payment.Name,
                Amount = payment.Amount,
                Currency = payment.Currency,
                DueDate = payment.DueDate,
                Complete = payment.Complete,
                Account = payment.Account.Id
            };

            return Ok(retViewModel);
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PaymentViewModel>> PostPayment(InitViewModel viewModel)
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

            // Find the specified account
            var account = await _context.Accounts
                .Include(x => x.Tenant)
                .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                .ConfigureAwait(false);
            if (account == null)
            {
                _logger.LogWarning("Could not find account for payment schedule");
                return BadRequest("Invalid account");
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
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
                CreatedUser = userId,
                UpdatedUser = Guid.Empty,
                DisabledUser = Guid.Empty
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

            // Build the ret view model
            var retViewModel = new PaymentViewModel
            {
                Id = payment.Id,
                Name = payment.Name,
                Amount = payment.Amount,
                Currency = payment.Currency,
                DueDate = payment.DueDate,
                Complete = payment.Complete,
                Account = payment.Account.Id
            };

            return CreatedAtAction("GetPayment", new {id = retViewModel.Id}, retViewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeletePayment(Guid id)
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

            // Find the specified payment
            var payment = await _context.Payments
                // Include the account and tenant
                .Include(x => x.Account)
                .ThenInclude(x => x.Tenant)
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (payment == null)
            {
                _logger.LogWarning($"User id {userId} attempted to delete unknown payment {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, payment.Account.Tenant.Id)
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