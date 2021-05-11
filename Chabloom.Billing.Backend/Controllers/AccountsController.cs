// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Models;
using Chabloom.Billing.Backend.Services;
using Chabloom.Billing.Backend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Billing.Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AccountsController : ControllerBase
    {
        private readonly BillingDbContext _context;
        private readonly ILogger<AccountsController> _logger;
        private readonly IValidator _validator;

        public AccountsController(BillingDbContext context, ILogger<AccountsController> logger,
            IValidator validator)
        {
            _context = context;
            _logger = logger;
            _validator = validator;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<AccountViewModel>>> GetAccounts(Guid tenantId)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, tenantId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access accounts");
                return Forbid();
            }

            // Get all accounts the user is authorized to view
            var accounts = await _context.Accounts
                .Where(x => x.TenantId == tenantId)
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (accounts == null)
            {
                return new List<AccountViewModel>();
            }

            // Convert to view models
            var viewModels = accounts
                .Select(x => new AccountViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address,
                    ReferenceId = x.ReferenceId,
                    TenantId = x.TenantId
                })
                .ToList();

            return Ok(viewModels);
        }

        [HttpGet("Authorized")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<AccountViewModel>>> GetAccountsAuthorized()
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Get all accounts the user is authorized to view
            var accounts = await _context.Accounts
                // Include the users
                .Include(x => x.Users)
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (!accounts.Any())
            {
                return new List<AccountViewModel>();
            }

            // Filter accounts by user id
            var authorizedAccounts = accounts
                .Where(x => x.Users.Select(y => y.UserId).Contains(userId))
                .ToList();
            if (!authorizedAccounts.Any())
            {
                return new List<AccountViewModel>();
            }

            // Convert to view models
            var viewModels = authorizedAccounts
                .Select(x => new AccountViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address,
                    ReferenceId = x.ReferenceId,
                    TenantId = x.TenantId
                })
                .ToList();

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<AccountViewModel>> GetAccount(Guid id)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified account if the user has access to it
            var viewModel = await _context.Accounts
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .Select(x => new AccountViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address,
                    ReferenceId = x.ReferenceId,
                    TenantId = x.TenantId
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown account {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access account {id}");
                return Forbid();
            }

            return Ok(viewModel);
        }

        [HttpGet("Reference/{id}")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<AccountViewModel>> GetAccountReference(string id, Guid tenantId)
        {
            // Find the specified account if the user has access to it
            var viewModel = await _context.Accounts
                .Where(x => x.TenantId == tenantId)
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .Select(x => new AccountViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address,
                    ReferenceId = x.ReferenceId,
                    TenantId = x.TenantId
                })
                .FirstOrDefaultAsync(x => x.ReferenceId == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User attempted to access unknown account {id}");
                return NotFound();
            }

            return Ok(viewModel);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AccountViewModel>> PutAccount(Guid id, AccountViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null || id != viewModel.Id)
            {
                return BadRequest();
            }

            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified account if the user has access to it
            var account = await _context.Accounts
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (account == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown account {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, account.TenantId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to update account {id}");
                return Forbid();
            }

            // Update the account
            account.Name = viewModel.Name;
            account.Address = viewModel.Address;
            account.ReferenceId = viewModel.ReferenceId;
            account.UpdatedUser = userId;
            account.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(account);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            _logger.LogInformation($"User {userId} updated account {id}");

            return Ok(new AccountViewModel
            {
                Id = account.Id,
                Name = account.Name,
                Address = account.Address,
                ReferenceId = account.ReferenceId,
                TenantId = account.TenantId
            });
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

            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, viewModel.TenantId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning(
                    $"User id {userId} was not authorized to create accounts for tenant {viewModel.TenantId}");
                return Forbid();
            }

            // Find the specified tenant
            var tenant = await _context.Tenants
                .FindAsync(viewModel.TenantId)
                .ConfigureAwait(false);
            if (tenant == null)
            {
                _logger.LogWarning($"Specified tenant {viewModel.TenantId} could not be found");
                return BadRequest();
            }

            // Create the new account
            var account = new Account
            {
                Name = viewModel.Name,
                Address = viewModel.Address,
                ReferenceId = viewModel.ReferenceId,
                Tenant = tenant,
                CreatedUser = userId
            };

            await _context.Accounts.AddAsync(account)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = account.Id;

            _logger.LogInformation($"User {userId} created account {account.Id} for tenant {tenant.Id}");

            return CreatedAtAction("GetAccount", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified account if the user has access to it
            var account = await _context.Accounts
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (account == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown account {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, account.TenantId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning(
                    $"User id {userId} was not authorized to delete accounts for tenant {account.TenantId}");
                return Forbid();
            }

            // Disable the account
            account.Disabled = true;
            account.DisabledUser = userId;
            account.DisabledTimestamp = DateTimeOffset.UtcNow;

            _context.Update(account);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}