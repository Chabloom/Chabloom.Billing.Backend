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
    public class BillSchedulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BillSchedulesController> _logger;

        public BillSchedulesController(ApplicationDbContext context, ILogger<BillSchedulesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<BillScheduleViewModel>>> GetBillSchedules(Guid? tenantId)
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

            // TODO: Query tenant for access

            List<BillScheduleViewModel> billSchedules;
            if (tenantId == null)
            {
                // Find all bill schedules the user has access to
                billSchedules = await _context.BillSchedules
                    .Include(x => x.Account)
                    .ThenInclude(x => x.Tenant)
                    .Include(x => x.Account)
                    .ThenInclude(x => x.Users)
                    .Where(x => x.Account.Users.Select(y => y.UserId).Contains(userId))
                    .Where(x => x.Account.Tenant.Id == tenantId)
                    .Select(x => new BillScheduleViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Amount = x.Amount,
                        DayDue = x.DayDue,
                        Interval = x.Interval,
                        Enabled = x.Enabled,
                        Account = x.Account.Id
                    })
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                // Find all bill schedules the user has access to
                billSchedules = await _context.BillSchedules
                    .Include(x => x.Account)
                    .ThenInclude(x => x.Users)
                    .Where(x => x.Account.Users.Select(y => y.UserId).Contains(userId))
                    .Select(x => new BillScheduleViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Amount = x.Amount,
                        DayDue = x.DayDue,
                        Interval = x.Interval,
                        Enabled = x.Enabled,
                        Account = x.Account.Id
                    })
                    .ToListAsync()
                    .ConfigureAwait(false);
            }

            return Ok(billSchedules);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<BillScheduleViewModel>> GetBillSchedule(Guid id)
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

            // TODO: Query tenant for access

            // Find the specified bill schedule if the user has access to it
            var account = await _context.BillSchedules
                .Include(x => x.Account)
                .ThenInclude(x => x.Users)
                .Where(x => x.Account.Users.Select(y => y.UserId).Contains(userId))
                .Select(x => new BillScheduleViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    DayDue = x.DayDue,
                    Interval = x.Interval,
                    Enabled = x.Enabled,
                    Account = x.Account.Id
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (account == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown bill schedule {id}");
                return NotFound();
            }

            return Ok(account);
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

            // Find the specified bill schedule
            var billSchedule = await _context.BillSchedules
                .Include(x => x.Account)
                .ThenInclude(x => x.Tenant)
                .ThenInclude(x => x.Users)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (billSchedule == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown bill schedule {id}");
                return NotFound();
            }

            // Ensure the current user belongs to the tenant
            if (!billSchedule.Account.Tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {billSchedule.Account.Tenant.Id}");
                return Forbid();
            }

            // Update the bill schedule
            billSchedule.Name = viewModel.Name;
            billSchedule.Amount = viewModel.Amount;
            billSchedule.DayDue = viewModel.DayDue;
            billSchedule.Interval = viewModel.Interval;
            billSchedule.Enabled = viewModel.Enabled;
            billSchedule.Account = await _context.Accounts
                .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                .ConfigureAwait(false);
            billSchedule.UpdatedUser = sid;
            billSchedule.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(billSchedule);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<BillScheduleViewModel>> PostBillSchedule(BillScheduleViewModel viewModel)
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

            // Create the new bill schedule
            var billSchedule = new BillSchedule
            {
                Name = viewModel.Name,
                Amount = viewModel.Amount,
                DayDue = viewModel.DayDue,
                Interval = viewModel.Interval,
                Enabled = viewModel.Enabled,
                Account = await _context.Accounts
                    .Include(x => x.Tenant)
                    .ThenInclude(x => x.Users)
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                    .ConfigureAwait(false),
                CreatedUser = sid,
                UpdatedUser = sid
            };

            // Ensure the account was found
            if (billSchedule.Account == null)
            {
                _logger.LogWarning($"Specified account {viewModel.Account} could not be found");
                return BadRequest();
            }

            // Ensure the current user belongs to the tenant
            if (!billSchedule.Account.Tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {billSchedule.Account.Tenant.Id}");
                return Forbid();
            }

            await _context.BillSchedules.AddAsync(billSchedule)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = billSchedule.Id;

            return CreatedAtAction("GetBillSchedule", new {id = viewModel.Id}, viewModel);
        }
    }
}