// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class PaymentSchedulesController : ControllerBase
    {
        private readonly PaymentServiceDbContext _context;

        public PaymentSchedulesController(PaymentServiceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [Authorize(Policy = "PaymentSchedule.Read")]
        public async Task<ActionResult<IEnumerable<PaymentScheduleViewModel>>> GetPaymentSchedules()
        {
            return await _context.PaymentSchedules
                .Include(x => x.Account)
                .Select(x => new PaymentScheduleViewModel
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
        [Authorize(Policy = "PaymentSchedule.Read")]
        public async Task<ActionResult<PaymentScheduleViewModel>> GetPaymentSchedule(Guid id)
        {
            var paymentSchedule = await _context.PaymentSchedules
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (paymentSchedule == null)
            {
                return NotFound();
            }

            return new PaymentScheduleViewModel
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
        [Authorize(Policy = "PaymentSchedule.Write")]
        public async Task<IActionResult> PutPaymentSchedule(Guid id, PaymentScheduleViewModel viewModel)
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
                .PaymentSchedules
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
        [Authorize(Policy = "PaymentSchedule.Write")]
        public async Task<ActionResult<PaymentScheduleViewModel>> PostPaymentSchedule(PaymentScheduleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null)
            {
                return BadRequest();
            }

            var paymentSchedule = new PaymentSchedule
            {
                Name = viewModel.Name,
                Amount = viewModel.Amount,
                DayDue = viewModel.DayDue,
                MonthInterval = viewModel.MonthInterval,
                Enabled = viewModel.Enabled,
                Account = await _context.Accounts
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                    .ConfigureAwait(false)
            };

            await _context.PaymentSchedules.AddAsync(paymentSchedule)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = paymentSchedule.Id;

            return CreatedAtAction("GetPaymentSchedule", new { id = viewModel.Id }, viewModel);
        }
    }
}