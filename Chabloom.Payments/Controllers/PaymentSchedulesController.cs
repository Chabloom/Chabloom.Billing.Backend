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
    public class PaymentSchedulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentSchedulesController> _logger;
        private readonly IValidator _validator;

        public PaymentSchedulesController(ApplicationDbContext context, ILogger<PaymentSchedulesController> logger,
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
        public async Task<ActionResult<IEnumerable<PaymentScheduleViewModel>>> GetPaymentSchedules(Guid accountId)
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, accountId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access payment schedules");
                return Forbid();
            }

            // Get all payment schedules
            var paymentSchedules = await _context.PaymentSchedules
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (paymentSchedules == null || !paymentSchedules.Any())
            {
                return new List<PaymentScheduleViewModel>();
            }

            // Convert to view models
            var viewModels = paymentSchedules
                .Select(x => new PaymentScheduleViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    Day = x.Day,
                    MonthInterval = x.MonthInterval,
                    BeginDate = x.BeginDate,
                    EndDate = x.EndDate,
                    TransactionScheduleId = x.TransactionScheduleId,
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
        public async Task<ActionResult<PaymentSchedule>> GetPaymentSchedule(Guid id)
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

            // Find the specified payment schedule
            var viewModel = await _context.PaymentSchedules
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .Select(x => new PaymentScheduleViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    Day = x.Day,
                    MonthInterval = x.MonthInterval,
                    BeginDate = x.BeginDate,
                    EndDate = x.EndDate,
                    TransactionScheduleId = x.TransactionScheduleId,
                    AccountId = x.AccountId
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown payment schedule {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, viewModel.AccountId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access payment schedule {id}");
                return Forbid();
            }

            // Log the operation
            _logger.LogInformation($"User {userId} read payment schedule {viewModel.Id}");

            return Ok(viewModel);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PaymentScheduleViewModel>> PutPaymentSchedule(Guid id,
            PaymentScheduleViewModel viewModel)
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
            var sid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

            var paymentSchedule = await _context.PaymentSchedules
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (paymentSchedule == null)
            {
                _logger.LogWarning($"User id {userId} attempted to update unknown payment schedule {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, paymentSchedule.Account.Tenant.Id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to update payment schedule {id}");
                return Forbid();
            }

            // Update the payment schedule
            paymentSchedule.Name = viewModel.Name;
            paymentSchedule.Amount = viewModel.Amount;
            paymentSchedule.Currency = viewModel.Currency;
            paymentSchedule.Day = viewModel.Day;
            paymentSchedule.MonthInterval = viewModel.MonthInterval;
            paymentSchedule.BeginDate = viewModel.BeginDate;
            paymentSchedule.EndDate = viewModel.EndDate;
            paymentSchedule.UpdatedUser = userId;
            paymentSchedule.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(paymentSchedule);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Log the operation
            _logger.LogInformation($"User {userId} updated payment schedule {paymentSchedule.Id}");

            return Ok(new PaymentScheduleViewModel
            {
                Id = paymentSchedule.Id,
                Name = paymentSchedule.Name,
                Amount = paymentSchedule.Amount,
                Currency = paymentSchedule.Currency,
                Day = paymentSchedule.Day,
                MonthInterval = paymentSchedule.MonthInterval,
                BeginDate = paymentSchedule.BeginDate,
                EndDate = paymentSchedule.EndDate,
                TransactionScheduleId = paymentSchedule.TransactionScheduleId,
                AccountId = paymentSchedule.AccountId
            });
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PaymentScheduleViewModel>> PostPaymentSchedule(
            PaymentScheduleViewModel viewModel)
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
            var sid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
                _logger.LogWarning($"User id {userId} was not authorized to create payment schedules");
                return Forbid();
            }

            // Create the new payment schedule
            var paymentSchedule = new PaymentSchedule
            {
                Name = viewModel.Name,
                Amount = viewModel.Amount,
                Day = viewModel.Day,
                MonthInterval = viewModel.MonthInterval,
                BeginDate = viewModel.BeginDate,
                EndDate = viewModel.EndDate,
                Account = account,
                CreatedUser = userId
            };

            // Optional currency specification
            if (!string.IsNullOrEmpty(viewModel.Currency))
            {
                paymentSchedule.Currency = viewModel.Currency;
            }

            await _context.PaymentSchedules.AddAsync(paymentSchedule)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Log the operation
            _logger.LogInformation($"User {userId} created payment schedule {paymentSchedule.Id}");

            viewModel.Id = paymentSchedule.Id;

            return CreatedAtAction("GetPaymentSchedule", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeletePaymentSchedule(Guid id)
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

            // Find the specified payment schedule
            var paymentSchedule = await _context.PaymentSchedules
                // Include the account
                .Include(x => x.Account)
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (paymentSchedule == null)
            {
                _logger.LogWarning($"User id {userId} attempted to delete unknown payment schedule {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, paymentSchedule.Account.TenantId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to delete payment schedule {id}");
                return Forbid();
            }

            // Disable the payment schedule
            paymentSchedule.Disabled = true;
            paymentSchedule.DisabledUser = userId;
            paymentSchedule.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(paymentSchedule);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}