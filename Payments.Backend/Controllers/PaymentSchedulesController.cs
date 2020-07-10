// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Backend.Data;
using Payments.Backend.Models;

namespace Payments.Backend.Controllers
{
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
        public async Task<ActionResult<IEnumerable<PaymentScheduleViewModel>>> GetPaymentSchedules()
        {
            return await _context.PaymentSchedules
                .Select(x => new PaymentScheduleViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    DayDue = x.DayDue,
                    Interval = x.Interval
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PaymentScheduleViewModel>> GetPaymentSchedule(Guid id)
        {
            var paymentSchedule = await _context.PaymentSchedules
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
                Interval = paymentSchedule.Interval
            };
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
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
            paymentSchedule.Interval = viewModel.Interval;

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

            await _context.PaymentSchedules.AddAsync(new PaymentSchedule
                {
                    Name = viewModel.Name,
                    Amount = viewModel.Amount,
                    DayDue = viewModel.DayDue,
                    Interval = viewModel.Interval
                })
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return CreatedAtAction("GetPaymentSchedule", new {id = viewModel.Id}, viewModel);
        }
    }
}