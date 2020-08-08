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
    public class BillsController : ControllerBase
    {
        private readonly PaymentServiceDbContext _context;

        public BillsController(PaymentServiceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [Authorize(Policy = "Bill.Read")]
        public async Task<ActionResult<IEnumerable<BillViewModel>>> GetBills()
        {
            return await _context.Bills
                .Include(x => x.Account)
                .Include(x => x.Transactions)
                .Select(x => new BillViewModel
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    DueDate = x.DueDate,
                    Account = x.Account.Id,
                    Transactions = x.Transactions
                        .Select(y => y.Id)
                        .ToList()
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [Authorize(Policy = "Bill.Read")]
        public async Task<ActionResult<BillViewModel>> GetBill(Guid id)
        {
            var bill = await _context.Bills
                .Include(x => x.Account)
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (bill == null)
            {
                return NotFound();
            }

            return new BillViewModel
            {
                Id = bill.Id,
                Amount = bill.Amount,
                DueDate = bill.DueDate,
                Account = bill.Account.Id,
                Transactions = bill.Transactions
                    .Select(x => x.Id)
                    .ToList()
            };
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [Authorize(Policy = "Bill.Write")]
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

            bill.Amount = viewModel.Amount;
            bill.DueDate = viewModel.DueDate;
            bill.Account = await _context.Accounts
                .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                .ConfigureAwait(false);

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
        [Authorize(Policy = "Bill.Write")]
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
                Amount = viewModel.Amount,
                DueDate = viewModel.DueDate,
                Account = await _context.Accounts
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                    .ConfigureAwait(false)
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