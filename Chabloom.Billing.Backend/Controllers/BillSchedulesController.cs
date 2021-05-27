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
        public async Task<IActionResult> GetBillSchedules(Guid accountId)
        {
            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            // Get all bill schedules
            var billSchedules = await _context.BillSchedules
                .Include(x => x.Account)
                .Where(x => x.AccountId == accountId)
                .Where(x => x.Account.TenantId == tenantId)
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .ToListAsync();
            if (billSchedules == null)
            {
                return Ok(new List<BillScheduleViewModel>());
            }

            var viewModels = billSchedules
                .Select(x => new BillScheduleViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    CurrencyId = x.CurrencyId,
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
        public async Task<IActionResult> GetBillSchedule(Guid id)
        {
            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            // Find the specified bill schedule
            var billSchedule = await _context.BillSchedules
                .FindAsync(id);
            if (billSchedule == null)
            {
                return NotFound();
            }

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(billSchedule, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            var retViewModel = new BillScheduleViewModel
            {
                Id = billSchedule.Id,
                Name = billSchedule.Name,
                Amount = billSchedule.Amount,
                CurrencyId = billSchedule.CurrencyId,
                Day = billSchedule.Day,
                MonthInterval = billSchedule.MonthInterval,
                BeginDate = billSchedule.BeginDate,
                EndDate = billSchedule.EndDate,
                TransactionScheduleId = billSchedule.TransactionScheduleId,
                AccountId = billSchedule.AccountId
            };

            return Ok(retViewModel);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutBillSchedule(Guid id, BillScheduleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            // Find the specified bill schedule
            var billSchedule = await _context.BillSchedules
                .FindAsync(id);
            if (billSchedule == null)
            {
                return NotFound();
            }

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(billSchedule, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            // Ensure the user is authorized at the requested level
            var (roleValid, roleResult) = await ValidateRoleAccessAsync(userId.Value, tenantId.Value);
            if (!roleValid)
            {
                return roleResult;
            }

            billSchedule.Name = viewModel.Name;
            billSchedule.Amount = viewModel.Amount;
            billSchedule.CurrencyId = viewModel.CurrencyId;
            billSchedule.Day = viewModel.Day;
            billSchedule.MonthInterval = viewModel.MonthInterval;
            billSchedule.BeginDate = viewModel.BeginDate;
            billSchedule.EndDate = viewModel.EndDate;
            billSchedule.UpdatedUser = userId.Value;
            billSchedule.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(billSchedule);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} updated bill schedule {billSchedule.Id}");

            var retViewModel = new BillScheduleViewModel
            {
                Id = billSchedule.Id,
                Name = billSchedule.Name,
                Amount = billSchedule.Amount,
                CurrencyId = billSchedule.CurrencyId,
                Day = billSchedule.Day,
                MonthInterval = billSchedule.MonthInterval,
                BeginDate = billSchedule.BeginDate,
                EndDate = billSchedule.EndDate,
                TransactionScheduleId = billSchedule.TransactionScheduleId,
                AccountId = billSchedule.AccountId
            };

            return Ok(retViewModel);
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> PostBillSchedule(BillScheduleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            var billSchedule = new BillSchedule
            {
                Name = viewModel.Name,
                Amount = viewModel.Amount,
                CurrencyId = viewModel.CurrencyId,
                Day = viewModel.Day,
                MonthInterval = viewModel.MonthInterval,
                BeginDate = viewModel.BeginDate,
                EndDate = viewModel.EndDate,
                AccountId = viewModel.AccountId,
                CreatedUser = userId.Value
            };

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(billSchedule, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            // Ensure the user is authorized at the requested level
            var (roleValid, roleResult) = await ValidateRoleAccessAsync(userId.Value, tenantId.Value);
            if (!roleValid)
            {
                return roleResult;
            }

            await _context.AddAsync(billSchedule);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} created bill schedule {billSchedule.Id}");

            var retViewModel = new BillScheduleViewModel
            {
                Id = billSchedule.Id,
                Name = billSchedule.Name,
                Amount = billSchedule.Amount,
                CurrencyId = billSchedule.CurrencyId,
                Day = billSchedule.Day,
                MonthInterval = billSchedule.MonthInterval,
                BeginDate = billSchedule.BeginDate,
                EndDate = billSchedule.EndDate,
                TransactionScheduleId = billSchedule.TransactionScheduleId,
                AccountId = billSchedule.AccountId
            };

            return Ok(retViewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteBillSchedule(Guid id)
        {
            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            // Find the specified bill schedule
            var billSchedule = await _context.BillSchedules
                .FindAsync(id);
            if (billSchedule == null)
            {
                _logger.LogWarning($"User id {userId} attempted to delete unknown bill schedule {id}");
                return NotFound();
            }

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(billSchedule, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            // Ensure the user is authorized at the requested level
            var (roleValid, roleResult) = await ValidateRoleAccessAsync(userId.Value, tenantId.Value);
            if (!roleValid)
            {
                return roleResult;
            }

            // Disable the bill schedule
            billSchedule.Disabled = true;
            billSchedule.DisabledUser = userId.Value;
            billSchedule.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(billSchedule);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} disabled bill schedule {billSchedule.Id}");

            return NoContent();
        }

        private async Task<Tuple<Guid?, Guid?, IActionResult>> GetUserTenantAsync()
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (!userId.HasValue)
            {
                return new Tuple<Guid?, Guid?, IActionResult>(null, null, Forbid());
            }

            // Get the current tenant id
            var tenantId = await _validator.GetTenantIdAsync(Request);
            if (!tenantId.HasValue)
            {
                return new Tuple<Guid?, Guid?, IActionResult>(null, null, Forbid());
            }

            return new Tuple<Guid?, Guid?, IActionResult>(userId, tenantId, Forbid());
        }

        private async Task<Tuple<bool, IActionResult>> ValidateTenantAsync(BillSchedule billSchedule, Guid tenantId)
        {
            // Find the specified account
            var account = await _context.Accounts
                .FindAsync(billSchedule.AccountId);
            if (account == null)
            {
                return new Tuple<bool, IActionResult>(false, NotFound());
            }

            // Ensure the user is calling this endpoint from the correct tenant
            if (account.TenantId != tenantId)
            {
                return new Tuple<bool, IActionResult>(false, Forbid());
            }

            return new Tuple<bool, IActionResult>(true, Ok());
        }

        private async Task<Tuple<bool, IActionResult>> ValidateRoleAccessAsync(Guid userId, Guid tenantId)
        {
            // Ensure the user is authorized at the requested level
            var userRoles = await _validator.GetTenantRolesAsync(userId, tenantId);
            if (!userRoles.Contains("Admin") &&
                !userRoles.Contains("Manager"))
            {
                _logger.LogWarning($"User id {userId} was not authorized to perform requested operation");
                return new Tuple<bool, IActionResult>(false, Forbid());
            }

            return new Tuple<bool, IActionResult>(true, Ok());
        }
    }
}