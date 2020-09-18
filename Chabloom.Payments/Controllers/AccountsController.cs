// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chabloom.Payments.Data;
using Chabloom.Payments.Models;
using Chabloom.Payments.Services;
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

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            bool userAuthorized;
            if (tenantId != null)
            {
                userAuthorized = await _validator.CheckTenantAccessAsync(userId, tenantId.Value)
                    .ConfigureAwait(false);
            }
            else
            {
                userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                    .ConfigureAwait(false);
            }

            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access accounts");
                return Forbid();
            }

            // Get all accounts the user is authorized to view
            var accounts = await _context.Accounts
                // Include the tenant
                .Include(x => x.Tenant)
                // Ensure the account has not been deleted
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (accounts == null)
            {
                return new List<AccountViewModel>();
            }

            List<AccountViewModel> viewModels;
            if (tenantId != null)
            {
                // Filter accounts by tenant id
                viewModels = accounts
                    .Where(x => x.Tenant.Id == tenantId)
                    .Select(x => new AccountViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ExternalId = x.ExternalId,
                        PrimaryAddress = x.PrimaryAddress,
                        Tenant = x.Tenant.Id
                    })
                    .ToList();
            }
            else
            {
                // Do not filter accounts
                viewModels = accounts
                    .Select(x => new AccountViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ExternalId = x.ExternalId,
                        PrimaryAddress = x.PrimaryAddress,
                        Tenant = x.Tenant.Id
                    })
                    .ToList();
            }

            return Ok(viewModels);
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

            // Find the specified account if the user has access to it
            var viewModel = await _context.Accounts
                // Include the tenant
                .Include(x => x.Tenant)
                // Ensure the account has not been deleted
                .Where(x => !x.Disabled)
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
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown account {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access account {id}");
                return Forbid();
            }

            return Ok(viewModel);
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
                .ThenInclude(x => x.Role)
                .Where(x => !x.Disabled)
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

            // Ensure the current user is a tenant admin or manager
            var tenantUser = account.Tenant.Users
                .FirstOrDefault(x => x.UserId == userId);
            if (tenantUser != null &&
                tenantUser.Role.Name != "Admin" &&
                tenantUser.Role.Name != "Manager")
            {
                _logger.LogWarning(
                    $"User id {userId} did not have permissions to update account for tenant {account.Tenant.Id}");
                return Forbid();
            }

            // Update the account
            account.Name = viewModel.Name;
            account.PrimaryAddress = viewModel.PrimaryAddress;
            account.UpdatedUser = userId;
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
                    .ThenInclude(x => x.Role)
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Tenant)
                    .ConfigureAwait(false),
                CreatedUser = userId,
                UpdatedUser = userId,
                DisabledUser = userId
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

            // Ensure the current user is a tenant admin or manager
            var tenantUser = account.Tenant.Users
                .FirstOrDefault(x => x.UserId == userId);
            if (tenantUser != null &&
                tenantUser.Role.Name != "Admin" &&
                tenantUser.Role.Name != "Manager")
            {
                _logger.LogWarning(
                    $"User id {userId} did not have permissions to create account for tenant {account.Tenant.Id}");
                return Forbid();
            }

            await _context.Accounts.AddAsync(account)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Create the account roles
            var roles = new List<AccountRole>
            {
                new AccountRole
                {
                    Name = "Admin",
                    Account = account,
                    CreatedUser = userId,
                    UpdatedUser = userId,
                    DisabledUser = userId
                },
                new AccountRole
                {
                    Name = "Manager",
                    Account = account,
                    CreatedUser = userId,
                    UpdatedUser = userId,
                    DisabledUser = userId
                },
                new AccountRole
                {
                    Name = "Basic",
                    Account = account,
                    CreatedUser = userId,
                    UpdatedUser = userId,
                    DisabledUser = userId
                }
            };

            await _context.AddRangeAsync(roles)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Add the user to the new account as an admin
            var accountUser = new AccountUser
            {
                UserId = userId,
                Account = account,
                Role = await _context.AccountRoles
                    .Where(x => x.Account == account)
                    .FirstOrDefaultAsync(x => x.Name == "Admin")
                    .ConfigureAwait(false),
                CreatedUser = userId,
                UpdatedUser = userId,
                DisabledUser = userId
            };

            await _context.AccountUsers.AddAsync(accountUser)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = account.Id;

            return CreatedAtAction("GetAccount", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAccount(Guid id)
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

            // Find the specified account if the user has access to it
            var account = await _context.Accounts
                .Include(x => x.Tenant)
                .ThenInclude(x => x.Users)
                .ThenInclude(x => x.Role)
                .Where(x => !x.Disabled)
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

            // Ensure the current user is a tenant admin or manager
            var tenantUser = account.Tenant.Users
                .FirstOrDefault(x => x.UserId == userId);
            if (tenantUser != null &&
                tenantUser.Role.Name != "Admin" &&
                tenantUser.Role.Name != "Manager")
            {
                _logger.LogWarning(
                    $"User id {userId} did not have permissions to disable account for tenant {account.Tenant.Id}");
                return Forbid();
            }

            // Disable the account
            account.Disabled = true;
            account.DisabledUser = userId;
            account.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(account);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}