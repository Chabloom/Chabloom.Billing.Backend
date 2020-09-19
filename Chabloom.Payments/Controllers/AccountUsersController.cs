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
    public class AccountUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountUsersController> _logger;
        private readonly IValidator _validator;

        public AccountUsersController(ApplicationDbContext context, ILogger<AccountUsersController> logger,
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
        public async Task<ActionResult<IEnumerable<AccountUserViewModel>>> GetAccountUsers(Guid? accountId,
            Guid? tenantId)
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
            if (accountId != null)
            {
                userAuthorized = await _validator.CheckAccountAccessAsync(userId, accountId.Value)
                    .ConfigureAwait(false);
            }
            else if (tenantId != null)
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
                _logger.LogWarning($"User id {userId} was not authorized to access account users");
                return Forbid();
            }

            // Get all account users
            var accountUsers = await _context.AccountUsers
                // Include the account and tenant
                .Include(x => x.Account)
                .ThenInclude(x => x.Tenant)
                // Include the role
                .Include(x => x.Role)
                // Ensure the account user has not been deleted
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (accountUsers == null || !accountUsers.Any())
            {
                return new List<AccountUserViewModel>();
            }

            List<AccountUserViewModel> viewModels;
            if (accountId != null)
            {
                // Filter account users by account id
                viewModels = accountUsers
                    .Where(x => x.Account.Id == accountId)
                    .Select(x => new AccountUserViewModel
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        Account = x.Account.Id,
                        AccountName = x.Account.Name,
                        Role = x.Role?.Id ?? Guid.Empty,
                        RoleName = x.Role?.Name
                    })
                    .Distinct()
                    .ToList();
            }
            else if (tenantId != null)
            {
                // Filter account users by tenant id
                viewModels = accountUsers
                    .Where(x => x.Account.Tenant.Id == tenantId)
                    .Select(x => new AccountUserViewModel
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        Account = x.Account.Id,
                        AccountName = x.Account.Name,
                        Role = x.Role?.Id ?? Guid.Empty,
                        RoleName = x.Role?.Name
                    })
                    .Distinct()
                    .ToList();
            }
            else
            {
                // Do not filter users
                viewModels = accountUsers
                    .Select(x => new AccountUserViewModel
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        Account = x.Account.Id,
                        AccountName = x.Account.Name,
                        Role = x.Role?.Id ?? Guid.Empty,
                        RoleName = x.Role?.Name
                    })
                    .Distinct()
                    .ToList();
            }

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<AccountUserViewModel>> GetAccountUser(Guid id)
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

            // Find the specified account user
            var viewModel = await _context.AccountUsers
                // Include the account
                .Include(x => x.Account)
                // Include the role
                .Include(x => x.Role)
                // Ensure the account user has not been deleted
                .Where(x => !x.Disabled)
                .Select(x => new AccountUserViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Account = x.Account.Id,
                    AccountName = x.Account.Name,
                    Role = x.Role.Id,
                    RoleName = x.Role.Name
                })
                .FirstOrDefaultAsync(x => x.UserId == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown account user {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, viewModel.Account)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access account user {id}");
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
        public async Task<IActionResult> PutAccountUser(Guid id, AccountUserViewModel viewModel)
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

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access account user {id}");
                return Forbid();
            }

            // Find the specified account user
            var accountUser = await _context.AccountUsers
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (accountUser == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown account user {id}");
                return NotFound();
            }

            // Update the account user
            accountUser.Role = await _context.AccountRoles
                .FirstOrDefaultAsync(x => x.Id == viewModel.Role)
                .ConfigureAwait(false);
            accountUser.UpdatedUser = userId;
            accountUser.UpdatedTimestamp = DateTimeOffset.UtcNow;

            // Ensure the account role was found
            if (accountUser.Role == null)
            {
                _logger.LogWarning($"Specified account role {viewModel.Role} could not be found");
                return BadRequest();
            }

            _context.Update(accountUser);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<AccountUserViewModel>> PostAccountUser(AccountUserViewModel viewModel)
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

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, viewModel.Id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to create account users");
                return Forbid();
            }

            // Create the new account user
            var accountUser = new AccountUser
            {
                UserId = viewModel.UserId,
                Account = await _context.Accounts
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                    .ConfigureAwait(false),
                Role = await _context.AccountRoles
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Role)
                    .ConfigureAwait(false),
                CreatedUser = userId,
                UpdatedUser = userId,
                DisabledUser = userId
            };

            // Ensure the account was found
            if (accountUser.Account == null)
            {
                _logger.LogWarning($"Specified account {viewModel.Account} could not be found");
                return BadRequest();
            }

            // Ensure the account role was found
            if (accountUser.Role == null)
            {
                _logger.LogWarning($"Specified account role {viewModel.Role} could not be found");
                return BadRequest();
            }

            await _context.AccountUsers.AddAsync(accountUser)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = accountUser.Id;

            return CreatedAtAction("GetAccountUser", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAccountUser(Guid id)
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
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to delete account user {id}");
                return Forbid();
            }

            // Find the specified account user
            var accountUser = await _context.AccountUsers
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (accountUser == null)
            {
                _logger.LogWarning($"User id {userId} attempted to delete unknown account user {id}");
                return NotFound();
            }

            // Disable the account user
            accountUser.Disabled = true;
            accountUser.DisabledUser = userId;
            accountUser.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(accountUser);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}