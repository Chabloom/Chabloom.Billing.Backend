// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Backend.Data;
using Payments.Backend.Models;

namespace Payments.Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BillSchedulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BillSchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<BillScheduleViewModel>>> GetBillSchedules()
        {
            return await _context.BillSchedules
                .Include(x => x.Account)
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
            var billSchedule = await _context.BillSchedules
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (billSchedule == null)
            {
                return NotFound();
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

            var billSchedule = await _context
                .BillSchedules
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (billSchedule == null)
            {
                return NotFound();
            }

            billSchedule.Name = viewModel.Name;
            billSchedule.Amount = viewModel.Amount;
            billSchedule.DayDue = viewModel.DayDue;
            billSchedule.MonthInterval = viewModel.MonthInterval;
            billSchedule.Enabled = viewModel.Enabled;
            billSchedule.Account = await _context.Accounts
                .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                .ConfigureAwait(false);
            billSchedule.UpdatedUser = User.GetDisplayName();
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
                CreatedUser = User.GetDisplayName(),
                UpdatedUser = User.GetDisplayName()
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