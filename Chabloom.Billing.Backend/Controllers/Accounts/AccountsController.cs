// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Models.Accounts;
using Chabloom.Billing.Backend.Services;
using Chabloom.Billing.Backend.ViewModels.Accounts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Billing.Backend.Controllers.Accounts
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            List<Account> accounts;
            // Ensure the user is authorized at the requested level
            var (roleValid, roleResult) = await ValidateRoleAccessAsync(userId.Value, tenantId.Value);
            if (roleValid)
            {
                // Get all accounts
                accounts = await _context.Accounts
                    .Where(x => x.TenantId == tenantId)
                    // Don't include disabled items
                    .Where(x => !x.DisabledTimestamp.HasValue)
                    .ToListAsync();
                if (accounts == null)
                {
                    return Ok(new List<AccountViewModel>());
                }
            }
            else
            {
                // Get all accounts the user is tracking
                accounts = await _context.UserAccounts
                    .Include(x => x.Account)
                    .Where(x => x.UserId == userId.Value)
                    .Select(x => x.Account)
                    .ToListAsync();
                if (accounts == null)
                {
                    return Ok(new List<AccountViewModel>());
                }
            }

            var viewModels = accounts
                .Select(x => new AccountViewModel
                {
                    Id = x.Id,
                    TenantLookupId = x.TenantLookupId,
                    Name = x.Name,
                    Address = x.Address,
                    TenantId = x.TenantId
                })
                .ToList();

            return Ok(viewModels);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetAccount(Guid id)
        {
            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            // Find the specified account
            var account = await _context.Accounts
                .FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(account, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            var retViewModel = new AccountViewModel
            {
                Id = account.Id,
                TenantLookupId = account.TenantLookupId,
                Name = account.Name,
                Address = account.Address,
                TenantId = account.TenantId
            };

            return Ok(retViewModel);
        }

        [AllowAnonymous]
        [HttpGet("Lookup/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> LookupAccount(string id)
        {
            // Get the user and tenant id
            var (_, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!tenantId.HasValue)
            {
                return userTenantResult;
            }

            // Find the specified account
            var account = await _context.Accounts
                .FirstOrDefaultAsync(x => x.TenantLookupId == id);
            if (account == null)
            {
                return NotFound();
            }

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(account, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            var retViewModel = new AccountViewModel
            {
                Id = account.Id,
                TenantLookupId = account.TenantLookupId,
                Name = account.Name,
                Address = account.Address,
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
        public async Task<IActionResult> PutAccount(Guid id, AccountViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            // Find the specified account
            var account = await _context.Accounts
                .FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(account, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            // Ensure the user is authorized at the requested level
            var (roleValid, roleResult) = await ValidateRoleAccessAsync(userId.Value, tenantId.Value);
            if (!roleValid)
            {
                return roleResult;
            }

            account.Name = viewModel.Name;
            account.TenantLookupId = viewModel.TenantLookupId;
            account.Address = viewModel.Address;
            account.UpdatedUser = userId.Value;
            account.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(account);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} updated account {account.Id}");

            var retViewModel = new AccountViewModel
            {
                Id = account.Id,
                TenantLookupId = account.TenantLookupId,
                Name = account.Name,
                Address = account.Address,
                TenantId = account.TenantId
            };

            return Ok(retViewModel);
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> PostAccount(AccountViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            var account = new Account
            {
                Name = viewModel.Name,
                TenantLookupId = viewModel.TenantLookupId,
                Address = viewModel.Address,
                TenantId = tenantId.Value,
                CreatedUser = userId.Value
            };

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(account, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            // Ensure the user is authorized at the requested level
            var (roleValid, roleResult) = await ValidateRoleAccessAsync(userId.Value, tenantId.Value);
            if (!roleValid)
            {
                return roleResult;
            }

            await _context.AddAsync(account);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} created account {account.Id}");

            var retViewModel = new AccountViewModel
            {
                Id = account.Id,
                TenantLookupId = account.TenantLookupId,
                Name = account.Name,
                Address = account.Address,
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
            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            // Find the specified account
            var account = await _context.Accounts
                .FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(account, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            // Ensure the user is authorized at the requested level
            var (roleValid, roleResult) = await ValidateRoleAccessAsync(userId.Value, tenantId.Value);
            if (!roleValid)
            {
                return roleResult;
            }

            account.DisabledUser = userId.Value;
            account.DisabledTimestamp = DateTimeOffset.UtcNow;

            _context.Update(account);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} disabled account {account.Id}");

            return NoContent();
        }

        private async Task<Tuple<Guid?, Guid?, IActionResult>> GetUserTenantAsync()
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (!userId.HasValue)
            {
                return new Tuple<Guid?, Guid?, IActionResult>(null, null, Forbid());
            }

            // Get the current tenant id
            var tenantId = await _validator.GetTenantIdAsync(Request);
            if (!tenantId.HasValue)
            {
                return new Tuple<Guid?, Guid?, IActionResult>(null, null, Forbid());
            }

            return new Tuple<Guid?, Guid?, IActionResult>(userId, tenantId, Forbid());
        }

        private Task<Tuple<bool, IActionResult>> ValidateTenantAsync(Account account, Guid tenantId)
        {
            // Ensure the user is calling this endpoint from the correct tenant
            if (account.TenantId != tenantId)
            {
                return Task.FromResult(new Tuple<bool, IActionResult>(false, Forbid()));
            }

            return Task.FromResult(new Tuple<bool, IActionResult>(true, Ok()));
        }

        private async Task<Tuple<bool, IActionResult>> ValidateRoleAccessAsync(Guid userId, Guid tenantId)
        {
            // Ensure the user is authorized at the requested level
            var userRoles = await _validator.GetTenantRolesAsync(userId, tenantId);
            if (!userRoles.Contains("Admin") &&
                !userRoles.Contains("Manager"))
            {
                _logger.LogWarning($"User id {userId} was not authorized to perform requested operation");
                return new Tuple<bool, IActionResult>(false, Forbid());
            }

            return new Tuple<bool, IActionResult>(true, Ok());
        }
    }
}