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
    public class ApplicationUsersController : ControllerBase
    {
        private readonly BillingDbContext _context;
        private readonly ILogger<ApplicationUsersController> _logger;
        private readonly IValidator _validator;

        public ApplicationUsersController(BillingDbContext context, ILogger<ApplicationUsersController> logger,
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
        public async Task<ActionResult<IEnumerable<ApplicationUserViewModel>>> GetApplicationUsers()
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access application users");
                return Forbid();
            }

            // Get all application users
            var applicationUsers = await _context.ApplicationUsers
                .ToListAsync()
                .ConfigureAwait(false);
            if (applicationUsers == null || !applicationUsers.Any())
            {
                return new List<ApplicationUserViewModel>();
            }

            // Convert to view models
            var viewModels = applicationUsers
                .Select(x => new ApplicationUserViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId
                })
                .Distinct()
                .ToList();

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApplicationUserViewModel>> GetApplicationUser(Guid id)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified application user
            var viewModel = await _context.ApplicationUsers
                .Select(x => new ApplicationUserViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId
                })
                .FirstOrDefaultAsync(x => x.UserId == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown application user {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access application user {id}");
                return Forbid();
            }

            return Ok(viewModel);
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApplicationUserViewModel>> PostApplicationUser(
            ApplicationUserViewModel viewModel)
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
            var userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to create application users");
                return Forbid();
            }

            // Create the new application user
            var applicationUser = new ApplicationUser
            {
                UserId = viewModel.UserId,
                CreatedUser = userId
            };

            await _context.ApplicationUsers.AddAsync(applicationUser)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = applicationUser.Id;

            _logger.LogInformation($"User {userId} added {viewModel.UserId} to application");

            return CreatedAtAction("GetApplicationUser", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteApplicationUser(Guid id)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to delete application user {id}");
                return Forbid();
            }

            // Find the specified application user
            var applicationUser = await _context.ApplicationUsers
                .FindAsync(id)
                .ConfigureAwait(false);
            if (applicationUser == null)
            {
                _logger.LogWarning($"User id {userId} attempted to delete unknown application user {id}");
                return NotFound();
            }

            _context.Remove(applicationUser);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            _logger.LogInformation($"User {userId} removed {applicationUser.UserId} from application");

            return NoContent();
        }
    }
}