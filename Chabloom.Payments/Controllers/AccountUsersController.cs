// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<ActionResult<IEnumerable<AccountUserViewModel>>> GetAccountUsers(Guid accountId)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, accountId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access account users");
                return Forbid();
            }

            // Get all account users
            var accountUsers = await _context.AccountUsers
                // For the specified account
                .Where(x => x.Account.Id == accountId)
                .ToListAsync()
                .ConfigureAwait(false);
            if (!accountUsers.Any())
            {
                return new List<AccountUserViewModel>();
            }

            // Convert to view models
            var viewModels = accountUsers
                .Select(x => new AccountUserViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    AccountId = x.AccountId
                })
                .Distinct()
                .ToList();

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<AccountUserViewModel>> GetAccountUser(Guid id)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified account user
            var viewModel = await _context.AccountUsers
                .Select(x => new AccountUserViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    AccountId = x.AccountId
                })
                .FirstOrDefaultAsync(x => x.UserId == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown account user {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, viewModel.AccountId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access account user {id}");
                return Forbid();
            }

            return Ok(viewModel);
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

            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified account
            var account = await _context.Accounts
                .FindAsync(viewModel.AccountId)
                .ConfigureAwait(false);
            if (account == null)
            {
                _logger.LogWarning($"Specified account {viewModel.AccountId} could not be found");
                return BadRequest();
            }

            // Create the new account user
            var accountUser = new AccountUser
            {
                UserId = viewModel.UserId,
                Account = account,
                CreatedUser = userId
            };

            await _context.AccountUsers.AddAsync(accountUser)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = accountUser.Id;

            _logger.LogInformation($"User {userId} added {viewModel.UserId} to account {account.Id}");

            return CreatedAtAction("GetAccountUser", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAccountUser(Guid id)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to delete account user {id}");
                return Forbid();
            }

            // Find the specified account user
            var accountUser = await _context.AccountUsers
                .FindAsync(id)
                .ConfigureAwait(false);
            if (accountUser == null)
            {
                _logger.LogWarning($"User id {userId} attempted to delete unknown account user {id}");
                return NotFound();
            }

            _context.Remove(accountUser);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            _logger.LogInformation($"User {userId} removed {accountUser.UserId} from account {accountUser.AccountId}");

            return NoContent();
        }
    }
}