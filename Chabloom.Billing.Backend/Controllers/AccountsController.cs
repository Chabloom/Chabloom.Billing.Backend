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
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountsController> _logger;
        private readonly IValidator _validator;

        public AccountsController(ApplicationDbContext context, ILogger<AccountsController> logger,
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
        public async Task<IActionResult> GetAccountsAsync()
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (!userId.HasValue)
            {
                return Forbid();
            }

            // Get the current tenant id
            var tenantId = await _validator.GetTenantIdAsync(Request);
            if (!tenantId.HasValue)
            {
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userRoles = await _validator.GetTenantRolesAsync(userId.Value, tenantId.Value);
            if (!userRoles.Contains("Admin") &&
                !userRoles.Contains("Manager"))
            {
                _logger.LogWarning($"User id {userId} was not authorized to access accounts");
                return Forbid();
            }

            // Get all accounts the user is authorized to view
            var accounts = await _context.Accounts
                .Where(x => x.TenantId == tenantId)
                // Don't include disabled items
                .Where(x => !x.Disabled)
                .ToListAsync();
            if (accounts == null)
            {
                return Ok(new List<AccountViewModel>());
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

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AccountViewModel>> GetAccount(Guid id)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (!userId.HasValue)
            {
                return Forbid();
            }

            // Get the current tenant id
            var tenantId = await _validator.GetTenantIdAsync(Request);
            if (!tenantId.HasValue)
            {
                return NotFound();
            }

            // Find the specified account
            var account = await _context.Accounts
                .FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            // Ensure the user is calling this endpoint from the correct tenant
            if (account.TenantId != tenantId)
            {
                return Forbid();
            }

            var retViewModel = new AccountViewModel
            {
                Id = account.Id,
                Name = account.Name,
                Address = account.Address,
                ReferenceId = account.ReferenceId,
                TenantId = account.TenantId
            };

            return Ok(retViewModel);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
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
            if (!userId.HasValue)
            {
                return Forbid();
            }

            // Get the current tenant id
            var tenantId = await _validator.GetTenantIdAsync(Request);
            if (!tenantId.HasValue)
            {
                return NotFound();
            }

            // Find the specified account
            var account = await _context.Accounts
                .FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userRoles = await _validator.GetTenantRolesAsync(userId.Value, tenantId.Value);
            if (!userRoles.Contains("Admin") &&
                !userRoles.Contains("Manager"))
            {
                _logger.LogWarning($"User id {userId} was not authorized to update accounts");
                return Forbid();
            }

            account.Name = viewModel.Name;
            account.Address = viewModel.Address;
            account.ReferenceId = viewModel.ReferenceId;
            account.UpdatedUser = userId.Value;
            account.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(account);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} updated account {account.Id}");

            var retViewModel = new AccountViewModel
            {
                Id = account.Id,
                Name = account.Name,
                Address = account.Address,
                ReferenceId = account.ReferenceId,
                TenantId = account.TenantId
            };

            return Ok(retViewModel);
        }

        [HttpPost]
        [ProducesResponseType(200)]
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
            if (!userId.HasValue)
            {
                return Forbid();
            }

            // Get the current tenant id
            var tenantId = await _validator.GetTenantIdAsync(Request);
            if (!tenantId.HasValue)
            {
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userRoles = await _validator.GetTenantRolesAsync(userId.Value, tenantId.Value);
            if (!userRoles.Contains("Admin") &&
                !userRoles.Contains("Manager"))
            {
                _logger.LogWarning($"User id {userId} was not authorized to update accounts");
                return Forbid();
            }

            var account = new Account
            {
                Name = viewModel.Name,
                Address = viewModel.Address,
                ReferenceId = viewModel.ReferenceId,
                TenantId = tenantId.Value,
                CreatedUser = userId.Value
            };

            await _context.AddAsync(account);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} created account {account.Id}");

            var retViewModel = new AccountViewModel
            {
                Id = account.Id,
                Name = account.Name,
                Address = account.Address,
                ReferenceId = account.ReferenceId,
                TenantId = account.TenantId
            };

            return Ok(retViewModel);
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
            if (!userId.HasValue)
            {
                return Forbid();
            }

            // Get the current tenant id
            var tenantId = await _validator.GetTenantIdAsync(Request);
            if (!tenantId.HasValue)
            {
                return NotFound();
            }

            // Find the specified account
            var account = await _context.Accounts
                .FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userRoles = await _validator.GetTenantRolesAsync(userId.Value, tenantId.Value);
            if (!userRoles.Contains("Admin") &&
                !userRoles.Contains("Manager"))
            {
                _logger.LogWarning($"User id {userId} was not authorized to update accounts");
                return Forbid();
            }

            account.Disabled = true;
            account.DisabledUser = userId.Value;
            account.DisabledTimestamp = DateTimeOffset.UtcNow;

            _context.Update(account);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}