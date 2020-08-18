// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chabloom.Payments.Data;
using Chabloom.Payments.Models;
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
        public async Task<ActionResult<IEnumerable<BillScheduleViewModel>>> GetBillSchedules()
        {
            // Get the current user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            return await _context.BillSchedules
                .Include(x => x.Account)
                .Where(x => x.Account.Owner == userId)
                .Select(x => new BillScheduleViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    DayDue = x.DayDue,
                    MonthInterval = x.MonthInterval,
                    Enabled = x.Enabled,
                    Account = x.Account.Id
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<BillScheduleViewModel>> GetBillSchedule(Guid id)
        {
            // Get the current user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            // Find the specified bill schedule
            var billSchedule = await _context.BillSchedules
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (billSchedule == null)
            {
                return NotFound();
            }

            // Ensure the user owns the account
            if (billSchedule.Account.Owner != userId)
            {
                return Forbid();
            }

            return new BillScheduleViewModel
            {
                Id = billSchedule.Id,
                Name = billSchedule.Name,
                Amount = billSchedule.Amount,
                DayDue = billSchedule.DayDue,
                MonthInterval = billSchedule.MonthInterval,
                Enabled = billSchedule.Enabled,
                Account = billSchedule.Account.Id
            };
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

            // Get the current user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            // Find the specified bill schedule
            var billSchedule = await _context
                .BillSchedules
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (billSchedule == null)
            {
                return NotFound();
            }

            // Ensure the user owns the account
            if (billSchedule.Account.Owner != userId)
            {
                return Forbid();
            }

            billSchedule.Name = viewModel.Name;
            billSchedule.Amount = viewModel.Amount;
            billSchedule.DayDue = viewModel.DayDue;
            billSchedule.MonthInterval = viewModel.MonthInterval;
            billSchedule.Enabled = viewModel.Enabled;
            billSchedule.Account = await _context.Accounts
                .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                .ConfigureAwait(false);
            billSchedule.UpdatedUser = userId;
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

            // Get the current user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            var billSchedule = new BillSchedule
            {
                Name = viewModel.Name,
                Amount = viewModel.Amount,
                DayDue = viewModel.DayDue,
                MonthInterval = viewModel.MonthInterval,
                Enabled = viewModel.Enabled,
                Account = await _context.Accounts
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                    .ConfigureAwait(false),
                CreatedUser = userId,
                UpdatedUser = userId
            };

            await _context.BillSchedules.AddAsync(billSchedule)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = billSchedule.Id;

            return CreatedAtAction("GetBillSchedule", new {id = viewModel.Id}, viewModel);
        }
    }
}