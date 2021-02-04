// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Models;
using Chabloom.Billing.Backend.Services;
using Chabloom.Billing.Backend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Billing.Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BillSchedulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BillSchedulesController> _logger;
        private readonly IValidator _validator;

        public BillSchedulesController(ApplicationDbContext context, ILogger<BillSchedulesController> logger,
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
        public async Task<ActionResult<IEnumerable<BillScheduleViewModel>>> GetBillSchedules(Guid accountId)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, accountId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access bill schedules");
                return Forbid();
            }

            // Get all bill schedules
            var billSchedules = await _context.BillSchedules
                .Where(x => x.AccountId == accountId)
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (billSchedules == null || !billSchedules.Any())
            {
                return new List<BillScheduleViewModel>();
            }

            // Convert to view models
            var viewModels = billSchedules
                .Select(x => new BillScheduleViewModel
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
        public async Task<ActionResult<BillSchedule>> GetBillSchedule(Guid id)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified bill schedule
            var viewModel = await _context.BillSchedules
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .Select(x => new BillScheduleViewModel
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
                _logger.LogWarning($"User id {userId} attempted to access unknown bill schedule {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, viewModel.AccountId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access bill schedule {id}");
                return Forbid();
            }

            // Log the operation
            _logger.LogInformation($"User {userId} read bill schedule {viewModel.Id}");

            return Ok(viewModel);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<BillScheduleViewModel>> PutBillSchedule(Guid id,
            BillScheduleViewModel viewModel)
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

            var billSchedule = await _context.BillSchedules
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (billSchedule == null)
            {
                _logger.LogWarning($"User id {userId} attempted to update unknown bill schedule {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, billSchedule.Account.Tenant.Id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to update bill schedule {id}");
                return Forbid();
            }

            // Update the bill schedule
            billSchedule.Name = viewModel.Name;
            billSchedule.Amount = viewModel.Amount;
            billSchedule.Currency = viewModel.Currency;
            billSchedule.Day = viewModel.Day;
            billSchedule.MonthInterval = viewModel.MonthInterval;
            billSchedule.BeginDate = viewModel.BeginDate;
            billSchedule.EndDate = viewModel.EndDate;
            billSchedule.UpdatedUser = userId;
            billSchedule.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(billSchedule);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Log the operation
            _logger.LogInformation($"User {userId} updated bill schedule {billSchedule.Id}");

            return Ok(new BillScheduleViewModel
            {
                Id = billSchedule.Id,
                Name = billSchedule.Name,
                Amount = billSchedule.Amount,
                Currency = billSchedule.Currency,
                Day = billSchedule.Day,
                MonthInterval = billSchedule.MonthInterval,
                BeginDate = billSchedule.BeginDate,
                EndDate = billSchedule.EndDate,
                TransactionScheduleId = billSchedule.TransactionScheduleId,
                AccountId = billSchedule.AccountId
            });
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<BillScheduleViewModel>> PostBillSchedule(
            BillScheduleViewModel viewModel)
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
                _logger.LogWarning("Could not find account for bill schedule");
                return BadRequest("Invalid account");
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, account.Tenant.Id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to create bill schedules");
                return Forbid();
            }

            // Create the new bill schedule
            var billSchedule = new BillSchedule
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
                billSchedule.Currency = viewModel.Currency;
            }

            await _context.BillSchedules.AddAsync(billSchedule)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Log the operation
            _logger.LogInformation($"User {userId} created bill schedule {billSchedule.Id}");

            viewModel.Id = billSchedule.Id;

            return CreatedAtAction("GetBillSchedule", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteBillSchedule(Guid id)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified bill schedule
            var billSchedule = await _context.BillSchedules
                // Include the account
                .Include(x => x.Account)
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (billSchedule == null)
            {
                _logger.LogWarning($"User id {userId} attempted to delete unknown bill schedule {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, billSchedule.Account.TenantId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to delete bill schedule {id}");
                return Forbid();
            }

            // Disable the bill schedule
            billSchedule.Disabled = true;
            billSchedule.DisabledUser = userId;
            billSchedule.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(billSchedule);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}