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
    public class SchedulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SchedulesController> _logger;
        private readonly IValidator _validator;

        public SchedulesController(ApplicationDbContext context, ILogger<SchedulesController> logger,
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
        public async Task<ActionResult<IEnumerable<ScheduleViewModel>>> GetSchedules(Guid? accountId, Guid? tenantId)
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
                _logger.LogWarning($"User id {userId} was not authorized to access schedules");
                return Forbid();
            }

            // Get all schedules the user is authorized to view
            var schedules = await _context.Schedules
                // Include the account and tenant
                .Include(x => x.Account)
                .ThenInclude(x => x.Tenant)
                // Ensure the schedule has not been deleted
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (schedules == null)
            {
                return new List<ScheduleViewModel>();
            }

            List<ScheduleViewModel> viewModels;
            if (accountId != null)
            {
                // Filter schedules by account id
                viewModels = schedules
                    .Where(x => x.Account.Id == accountId)
                    .Select(x => new ScheduleViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Amount = x.Amount,
                        DayDue = x.DayDue,
                        Interval = x.Interval,
                        Account = x.Account.Id
                    })
                    .ToList();
            }
            else if (tenantId != null)
            {
                // Filter schedules by tenant id
                viewModels = schedules
                    .Where(x => x.Account.Tenant.Id == tenantId)
                    .Select(x => new ScheduleViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Amount = x.Amount,
                        DayDue = x.DayDue,
                        Interval = x.Interval,
                        Account = x.Account.Id
                    })
                    .ToList();
            }
            else
            {
                // Do not filter schedules
                viewModels = schedules
                    .Select(x => new ScheduleViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Amount = x.Amount,
                        DayDue = x.DayDue,
                        Interval = x.Interval,
                        Account = x.Account.Id
                    })
                    .ToList();
            }

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ScheduleViewModel>> GetSchedule(Guid id)
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

            // Find the specified schedule if the user has access to it
            var viewModel = await _context.Schedules
                // Include the account
                .Include(x => x.Account)
                // Ensure the schedule has not been deleted
                .Where(x => !x.Disabled)
                .Select(x => new ScheduleViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    DayDue = x.DayDue,
                    Interval = x.Interval,
                    Account = x.Account.Id
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown schedule {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, viewModel.Account)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access schedule {id}");
                return Forbid();
            }

            return Ok(viewModel);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutSchedule(Guid id, ScheduleViewModel viewModel)
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

            // Find the specified schedule
            var schedule = await _context.Schedules
                .Include(x => x.Account)
                .ThenInclude(x => x.Tenant)
                .ThenInclude(x => x.Users)
                .ThenInclude(x => x.Role)
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (schedule == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown schedule {id}");
                return NotFound();
            }

            // Ensure the current user belongs to the tenant
            if (!schedule.Account.Tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {schedule.Account.Tenant.Id}");
                return Forbid();
            }

            // Ensure the current user is a tenant admin or manager
            var tenantUser = schedule.Account.Tenant.Users
                .FirstOrDefault(x => x.UserId == userId);
            if (tenantUser != null &&
                tenantUser.Role.Name != "Admin" &&
                tenantUser.Role.Name != "Manager")
            {
                _logger.LogWarning(
                    $"User id {userId} did not have permissions to update schedule for tenant {schedule.Account.Tenant.Id}");
                return Forbid();
            }

            // Update the schedule
            schedule.Name = viewModel.Name;
            schedule.Amount = viewModel.Amount;
            schedule.DayDue = viewModel.DayDue;
            schedule.Interval = viewModel.Interval;
            schedule.UpdatedUser = userId;
            schedule.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(schedule);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ScheduleViewModel>> PostSchedule(ScheduleViewModel viewModel)
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

            // Create the new schedule
            var schedule = new Schedule
            {
                Name = viewModel.Name,
                Amount = viewModel.Amount,
                DayDue = viewModel.DayDue,
                Interval = viewModel.Interval,
                Account = await _context.Accounts
                    .Include(x => x.Tenant)
                    .ThenInclude(x => x.Users)
                    .ThenInclude(x => x.Role)
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                    .ConfigureAwait(false),
                CreatedUser = userId,
                UpdatedUser = userId,
                DisabledUser = userId
            };

            // Ensure the account was found
            if (schedule.Account == null)
            {
                _logger.LogWarning($"Specified account {viewModel.Account} could not be found");
                return BadRequest();
            }

            // Ensure the current user belongs to the tenant
            if (!schedule.Account.Tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {schedule.Account.Tenant.Id}");
                return Forbid();
            }

            // Ensure the current user is a tenant admin or manager
            var tenantUser = schedule.Account.Tenant.Users
                .FirstOrDefault(x => x.UserId == userId);
            if (tenantUser != null &&
                tenantUser.Role.Name != "Admin" &&
                tenantUser.Role.Name != "Manager")
            {
                _logger.LogWarning(
                    $"User id {userId} did not have permissions to create schedule for tenant {schedule.Account.Tenant.Id}");
                return Forbid();
            }

            await _context.Schedules.AddAsync(schedule)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = schedule.Id;

            return CreatedAtAction("GetSchedule", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteSchedule(Guid id)
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

            // Find the specified schedule
            var schedule = await _context.Schedules
                .Include(x => x.Account)
                .ThenInclude(x => x.Tenant)
                .ThenInclude(x => x.Users)
                .ThenInclude(x => x.Role)
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (schedule == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown schedule {id}");
                return NotFound();
            }

            // Ensure the current user belongs to the tenant
            if (!schedule.Account.Tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {schedule.Account.Tenant.Id}");
                return Forbid();
            }

            // Ensure the current user is a tenant admin or manager
            var tenantUser = schedule.Account.Tenant.Users
                .FirstOrDefault(x => x.UserId == userId);
            if (tenantUser != null &&
                tenantUser.Role.Name != "Admin" &&
                tenantUser.Role.Name != "Manager")
            {
                _logger.LogWarning(
                    $"User id {userId} did not have permissions to disable schedule for tenant {schedule.Account.Tenant.Id}");
                return Forbid();
            }

            // Disable the schedule
            schedule.Disabled = true;
            schedule.DisabledUser = userId;
            schedule.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(schedule);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}