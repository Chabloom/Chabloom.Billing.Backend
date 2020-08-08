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
        [Authorize(Policy = "Account.Read")]
        public async Task<ActionResult<IEnumerable<AccountViewModel>>> GetAccounts()
        {
            return await _context.Accounts
                .Include(x => x.PaymentSchedules)
                .Include(x => x.Bills)
                .Include(x => x.Partition)
                .Select(x => new AccountViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    PrimaryAddress = x.PrimaryAddress,
                    Enabled = x.Enabled,
                    PaymentSchedules = x.PaymentSchedules
                        .Select(y => y.Id)
                        .ToList(),
                    Bills = x.Bills
                        .Select(y => y.Id)
                        .ToList(),
                    Partition = x.Partition.Id
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [Authorize(Policy = "Account.Read")]
        public async Task<ActionResult<AccountViewModel>> GetAccount(Guid id)
        {
            var account = await _context.Accounts
                .Include(x => x.PaymentSchedules)
                .Include(x => x.Bills)
                .Include(x => x.Partition)
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
                Enabled = account.Enabled,
                PaymentSchedules = account.PaymentSchedules
                    .Select(x => x.Id)
                    .ToList(),
                Bills = account.Bills
                    .Select(x => x.Id)
                    .ToList(),
                Partition = account.Partition.Id
            };
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [Authorize(Policy = "Account.Write")]
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
            account.Enabled = viewModel.Enabled;
            account.Partition = await _context.Partitions
                .FirstOrDefaultAsync(x => x.Id == viewModel.Partition)
                .ConfigureAwait(false);

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
        [Authorize(Policy = "Account.Write")]
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

            // Assign to default partition if the partition was not specified
            if (viewModel.Partition == Guid.Empty)
            {
                viewModel.Partition = Guid.Parse("040B9DA1-352D-4B81-8E6A-78ECD2FF14D1");
            }

            var account = new Account
            {
                Name = viewModel.Name,
                PrimaryAddress = viewModel.PrimaryAddress,
                Enabled = viewModel.Enabled,
                Partition = await _context.Partitions
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Partition)
                    .ConfigureAwait(false)
            };

            await _context.Accounts.AddAsync(account)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = account.Id;

            return CreatedAtAction("GetAccount", new {id = viewModel.Id}, viewModel);
        }
    }
}