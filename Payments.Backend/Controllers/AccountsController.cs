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
    public class AccountsController : ControllerBase
    {
        private readonly PaymentServiceDbContext _context;

        public AccountsController(PaymentServiceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<AccountViewModel>>> GetAccounts()
        {
            return await _context.Accounts
                .Select(x => new AccountViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    PrimaryAddress = x.PrimaryAddress,
                    Amount = x.Amount,
                    DayDue = x.DayDue,
                    Interval = x.Interval,
                    Enabled = x.Enabled
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<AccountViewModel>> GetAccount(Guid id)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (account == null)
            {
                return NotFound();
            }

            return new AccountViewModel
            {
                Id = account.Id,
                Name = account.Name,
                PrimaryAddress = account.PrimaryAddress,
                Amount = account.Amount,
                DayDue = account.DayDue,
                Interval = account.Interval,
                Enabled = account.Enabled
            };
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutAccount(Guid id, AccountViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null || id != viewModel.Id)
            {
                return BadRequest();
            }

            var account = await _context
                .Accounts
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (account == null)
            {
                return NotFound();
            }

            account.Name = viewModel.Name;
            account.PrimaryAddress = viewModel.PrimaryAddress;
            account.Amount = viewModel.Amount;
            account.DayDue = viewModel.DayDue;
            account.Interval = viewModel.Interval;
            account.Enabled = viewModel.Enabled;

            _context.Update(account);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<AccountViewModel>> PostAccount(AccountViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null)
            {
                return BadRequest();
            }

            await _context.Accounts.AddAsync(new Account
                {
                    Name = viewModel.Name,
                    PrimaryAddress = viewModel.PrimaryAddress,
                    Amount = viewModel.Amount,
                    DayDue = viewModel.DayDue,
                    Interval = viewModel.Interval,
                    Enabled = viewModel.Enabled
                })
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return CreatedAtAction("GetAccount", new {id = viewModel.Id}, viewModel);
        }
    }
}