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
        public async Task<IActionResult> GetUserAccounts([FromQuery] Guid tenantId)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Get all accounts the user is tracking
            var userAccounts = await _context.UserAccounts
                .Include(x => x.AccountId)
                .Where(x => x.Account.TenantId == tenantId)
                .Where(x => x.UserId == userId)
                .ToListAsync();
            if (userAccounts == null)
            {
                return Ok(new List<UserAccountViewModel>());
            }

            // Convert to view models
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
        public async Task<IActionResult> CreateUserAccount([FromBody] UserAccountViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified account
            var account = await _context.Accounts
                .FindAsync(viewModel.AccountId);
            if (account == null)
            {
                return BadRequest();
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
                UserId = userId,
                AccountId = viewModel.AccountId
            };

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
        public async Task<IActionResult> DeleteUserAccount([FromBody] UserAccountViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified user account
            var userAccount = await _context.UserAccounts
                .Where(x => x.AccountId == viewModel.AccountId)
                .FirstOrDefaultAsync(x => x.UserId == userId);
            if (userAccount == null)
            {
                return NotFound();
            }

            _context.Remove(userAccount);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} stopped tracking account {userAccount.AccountId}");

            return Ok();
        }
    }
}