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
        public async Task<ActionResult<IEnumerable<AccountViewModel>>> GetAccounts()
        {
            return await _context.Accounts
                .Select(x => x.ToViewModel())
                .ToListAsync()
                .ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AccountViewModel>> GetAccount(Guid id)
        {
            var account = await _context.Accounts.FindAsync(id)
                .ConfigureAwait(false);

            if (account == null)
            {
                return NotFound();
            }

            return account.ToViewModel();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccount(Guid id, AccountViewModel viewModel)
        {
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

            account = viewModel.ToModel();

            _context.Update(account);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<AccountViewModel>> PostAccount(AccountViewModel account)
        {
            if (account == null)
            {
                return BadRequest();
            }

            await _context.Accounts.AddAsync(account.ToModel())
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return CreatedAtAction("GetAccount", new {id = account.Id}, account);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<AccountViewModel>> DeleteAccount(Guid id)
        {
            var account = await _context.Accounts.FindAsync(id)
                .ConfigureAwait(false);
            if (account == null)
            {
                return NotFound();
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return account.ToViewModel();
        }
    }
}