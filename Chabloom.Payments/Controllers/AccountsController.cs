// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chabloom.Payments.Data;
using Chabloom.Payments.Models;
using Chabloom.Payments.ViewModels;
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
    public class AccountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(ApplicationDbContext context, ILogger<AccountsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<AccountViewModel>>> GetAccounts(Guid? tenantId)
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("User attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"User sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // TODO: Query tenant for access

            List<AccountViewModel> accounts;
            if (tenantId == null)
            {
                // Find all accounts the user has access to
                accounts = await _context.Accounts
                    .Include(x => x.Tenant)
                    .Include(x => x.Users)
                    .Where(x => x.Users.Select(y => y.UserId).Contains(userId))
                    .Select(x => new AccountViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ExternalId = x.ExternalId,
                        PrimaryAddress = x.PrimaryAddress,
                        Tenant = x.Tenant.Id
                    })
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                // Find all accounts the user has access to
                accounts = await _context.Accounts
                    .Include(x => x.Tenant)
                    .Include(x => x.Users)
                    .Where(x => x.Users.Select(y => y.UserId).Contains(userId))
                    .Where(x => x.Tenant.Id == tenantId)
                    .Select(x => new AccountViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ExternalId = x.ExternalId,
                        PrimaryAddress = x.PrimaryAddress,
                        Tenant = x.Tenant.Id
                    })
                    .ToListAsync()
                    .ConfigureAwait(false);
            }

            return Ok(accounts);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<AccountViewModel>> GetAccount(Guid id)
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("User attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"User sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // TODO: Query tenant for access

            // Find the specified account if the user has access to it
            var account = await _context.Accounts
                .Include(x => x.Tenant)
                .Include(x => x.Users)
                .Where(x => x.Users.Select(y => y.UserId).Contains(userId))
                .Select(x => new AccountViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ExternalId = x.ExternalId,
                    PrimaryAddress = x.PrimaryAddress,
                    Tenant = x.Tenant.Id
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (account == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown account {id}");
                return NotFound();
            }

            return Ok(account);
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

            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("User attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"User sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // Find the specified account if the user has access to it
            var account = await _context.Accounts
                .Include(x => x.Tenant)
                .ThenInclude(x => x.Users)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (account == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown account {id}");
                return NotFound();
            }

            // Ensure the current user belongs to the tenant
            if (!account.Tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {account.Tenant.Id}");
                return Forbid();
            }

            // Update the account
            account.Name = viewModel.Name;
            account.PrimaryAddress = viewModel.PrimaryAddress;
            account.UpdatedUser = sid;
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

            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("User attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"User sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // Create the new account
            var account = new Account
            {
                Name = viewModel.Name,
                ExternalId = viewModel.ExternalId,
                PrimaryAddress = viewModel.PrimaryAddress,
                Tenant = await _context.Tenants
                    .Include(x => x.Users)
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Tenant)
                    .ConfigureAwait(false),
                CreatedUser = sid,
                UpdatedUser = sid
            };

            // Ensure the tenant was found
            if (account.Tenant == null)
            {
                _logger.LogWarning($"Specified tenant {viewModel.Tenant} could not be found");
                return BadRequest();
            }

            // Ensure the current user belongs to the tenant
            if (!account.Tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {account.Tenant.Id}");
                return Forbid();
            }

            // TODO: Ensure the user is able to create accounts

            await _context.Accounts.AddAsync(account)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Add the user to the new account
            var accountUser = new AccountUser
            {
                UserId = userId,
                Account = account,
                CreatedUser = sid,
                UpdatedUser = sid
            };

            await _context.AccountUsers.AddAsync(accountUser)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = account.Id;

            return CreatedAtAction("GetAccount", new {id = viewModel.Id}, viewModel);
        }
    }
}