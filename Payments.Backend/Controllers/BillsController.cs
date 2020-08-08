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
    public class BillsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BillsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<BillViewModel>>> GetBills()
        {
            return await _context.Bills
                .Include(x => x.Account)
                .Include(x => x.BillSchedule)
                .Select(x => new BillViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    DueDate = x.DueDate,
                    Account = x.Account.Id,
                    BillSchedule = x.BillSchedule.Id
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<BillViewModel>> GetBill(Guid id)
        {
            var bill = await _context.Bills
                .Include(x => x.Account)
                .Include(x => x.BillSchedule)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (bill == null)
            {
                return NotFound();
            }

            return new BillViewModel
            {
                Id = bill.Id,
                Name = bill.Name,
                Amount = bill.Amount,
                DueDate = bill.DueDate,
                Account = bill.Account.Id,
                BillSchedule = bill.BillSchedule.Id
            };
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutBill(Guid id, BillViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null || id != viewModel.Id)
            {
                return BadRequest();
            }

            var bill = await _context
                .Bills
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (bill == null)
            {
                return NotFound();
            }

            bill.Name = viewModel.Name;
            bill.Amount = viewModel.Amount;
            bill.DueDate = viewModel.DueDate;
            bill.Account = await _context.Accounts
                .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                .ConfigureAwait(false);
            bill.BillSchedule = await _context.BillSchedules
                .FirstOrDefaultAsync(x => x.Id == viewModel.BillSchedule)
                .ConfigureAwait(false);
            bill.UpdatedUser = User.GetDisplayName();
            bill.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(bill);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<BillViewModel>> PostBill(BillViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null)
            {
                return BadRequest();
            }

            var bill = new Bill
            {
                Name = viewModel.Name,
                Amount = viewModel.Amount,
                DueDate = viewModel.DueDate,
                Account = await _context.Accounts
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                    .ConfigureAwait(false),
                BillSchedule = await _context.BillSchedules
                    .FirstOrDefaultAsync(x => x.Id == viewModel.BillSchedule)
                    .ConfigureAwait(false),
                CreatedUser = User.GetDisplayName(),
                UpdatedUser = User.GetDisplayName()
            };

            await _context.Bills.AddAsync(bill)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = bill.Id;

            return CreatedAtAction("GetBill", new {id = viewModel.Id}, viewModel);
        }
    }
}