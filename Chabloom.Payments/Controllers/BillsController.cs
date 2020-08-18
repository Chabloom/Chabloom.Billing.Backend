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
    public class BillsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BillsController> _logger;

        public BillsController(ApplicationDbContext context, ILogger<BillsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<BillViewModel>>> GetBills()
        {
            // Get the current user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

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
            // Get the current user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            // Find the specified bill
            var bill = await _context.Bills
                .Include(x => x.Account)
                .Include(x => x.BillSchedule)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (bill == null)
            {
                return NotFound();
            }

            // Ensure the user owns the account
            if (bill.Account.Owner != userId)
            {
                return Forbid();
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

            // Get the current user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            // Find the specified bill
            var bill = await _context.Bills
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (bill == null)
            {
                return NotFound();
            }

            // Ensure the user owns the account
            if (bill.Account.Owner != userId)
            {
                return Forbid();
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
            bill.UpdatedUser = userId;
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

            // Get the current user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
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
                CreatedUser = userId,
                UpdatedUser = userId
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