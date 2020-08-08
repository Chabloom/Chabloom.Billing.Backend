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
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
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
        [Authorize(Policy = "BillSchedule.Read")]
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
        [Authorize(Policy = "BillSchedule.Read")]
        public async Task<ActionResult<BillScheduleViewModel>> GetBillSchedule(Guid id)
        {
            var paymentSchedule = await _context.BillSchedules
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (paymentSchedule == null)
            {
                return NotFound();
            }

            return new BillScheduleViewModel
            {
                Id = paymentSchedule.Id,
                Name = paymentSchedule.Name,
                Amount = paymentSchedule.Amount,
                DayDue = paymentSchedule.DayDue,
                MonthInterval = paymentSchedule.MonthInterval,
                Enabled = paymentSchedule.Enabled,
                Account = paymentSchedule.Account.Id
            };
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [Authorize(Policy = "BillSchedule.Write")]
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

            var paymentSchedule = await _context
                .BillSchedules
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (paymentSchedule == null)
            {
                return NotFound();
            }

            paymentSchedule.Name = viewModel.Name;
            paymentSchedule.Amount = viewModel.Amount;
            paymentSchedule.DayDue = viewModel.DayDue;
            paymentSchedule.MonthInterval = viewModel.MonthInterval;
            paymentSchedule.Enabled = viewModel.Enabled;
            paymentSchedule.Account = await _context.Accounts
                .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                .ConfigureAwait(false);
            paymentSchedule.UpdatedUser = User.GetDisplayName();
            paymentSchedule.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(paymentSchedule);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [Authorize(Policy = "BillSchedule.Write")]
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

            var paymentSchedule = new BillSchedule
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

            await _context.BillSchedules.AddAsync(paymentSchedule)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = paymentSchedule.Id;

            return CreatedAtAction("GetBillSchedule", new { id = viewModel.Id }, viewModel);
        }
    }
}