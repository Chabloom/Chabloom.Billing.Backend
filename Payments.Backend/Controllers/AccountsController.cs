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
    public class AccountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountsController(ApplicationDbContext context)
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
                .Include(x => x.Owner)
                .Select(x => new AccountViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    PrimaryAddress = x.PrimaryAddress,
                    ExternalId = x.ExternalId,
                    OwnerId = x.Owner.Id
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
                .Include(x => x.Owner)
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
                ExternalId = account.ExternalId,
                OwnerId = account.Owner.Id
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
            account.ExternalId = viewModel.ExternalId;
            account.Owner = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == viewModel.OwnerId)
                .ConfigureAwait(false);
            account.UpdatedUser = User.GetDisplayName();
            account.UpdatedTimestamp = DateTimeOffset.UtcNow;

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

            var account = new Account
            {
                Name = viewModel.Name,
                PrimaryAddress = viewModel.PrimaryAddress,
                ExternalId = viewModel.ExternalId,
                Owner = await _context.Users
                    .FirstOrDefaultAsync(x => x.Id == viewModel.OwnerId)
                    .ConfigureAwait(false),
                CreatedUser = User.GetDisplayName(),
                UpdatedUser = User.GetDisplayName()
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