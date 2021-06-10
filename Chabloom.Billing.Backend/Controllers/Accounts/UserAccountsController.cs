// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Models.Accounts;
using Chabloom.Billing.Backend.Services;
using Chabloom.Billing.Backend.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Billing.Backend.Controllers.Accounts
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserAccountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserAccountsController> _logger;
        private readonly IValidator _validator;

        public UserAccountsController(ApplicationDbContext context, ILogger<UserAccountsController> logger,
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
        public async Task<IActionResult> GetUserAccountsAsync()
        {
            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            // Get all user accounts
            var userAccounts = await _context.UserAccounts
                .Include(x => x.Account)
                .Where(x => x.Account.TenantId == tenantId)
                .Where(x => x.UserId == userId)
                .ToListAsync();
            if (userAccounts == null)
            {
                return Ok(new List<UserAccountViewModel>());
            }

            var viewModels = userAccounts
                .Select(x => new UserAccountViewModel
                {
                    UserId = x.UserId,
                    AccountId = x.AccountId
                })
                .ToList();

            return Ok(viewModels);
        }

        [HttpPost("Create")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> CreateUserAccountAsync([FromBody] UserAccountViewModel viewModel)
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

            // Ensure the user account does not yet exist
            var userAccount = await _context.UserAccounts
                .Where(x => x.AccountId == viewModel.AccountId)
                .FirstOrDefaultAsync(x => x.UserId == userId);
            if (userAccount != null)
            {
                return Conflict();
            }

            // Create the new account
            userAccount = new UserAccount
            {
                UserId = viewModel.UserId,
                AccountId = viewModel.AccountId
            };

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(userAccount, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            await _context.AddAsync(userAccount);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} started tracking account {userAccount.AccountId}");

            return Ok();
        }

        [HttpPost("Delete")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUserAccountAsync([FromBody] UserAccountViewModel viewModel)
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

            // Find the specified user account
            var userAccount = await _context.UserAccounts
                .Where(x => x.AccountId == viewModel.AccountId)
                .FirstOrDefaultAsync(x => x.UserId == userId);
            if (userAccount == null)
            {
                return NotFound();
            }

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(userAccount, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            _context.Remove(userAccount);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} stopped tracking account {userAccount.AccountId}");

            return Ok();
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

        private async Task<Tuple<bool, IActionResult>> ValidateTenantAsync(UserAccount userAccount, Guid tenantId)
        {
            // Find the specified account
            var account = await _context.Accounts
                .FindAsync(userAccount.AccountId);
            if (account == null)
            {
                return new Tuple<bool, IActionResult>(false, NotFound());
            }

            // Ensure the user is calling this endpoint from the correct tenant
            if (account.TenantId != tenantId)
            {
                return new Tuple<bool, IActionResult>(false, Forbid());
            }

            return new Tuple<bool, IActionResult>(true, Ok());
        }
    }
}