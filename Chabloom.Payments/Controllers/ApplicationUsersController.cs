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
    public class ApplicationUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApplicationUsersController> _logger;
        private readonly IValidator _validator;

        public ApplicationUsersController(ApplicationDbContext context, ILogger<ApplicationUsersController> logger,
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
            var userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access application users");
                return Forbid();
            }

            // Get all application users
            var applicationUsers = await _context.ApplicationUsers
                // Include role
                .Include(x => x.Role)
                // Ensure the application user has not been deleted
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (applicationUsers == null || !applicationUsers.Any())
            {
                return new List<ApplicationUserViewModel>();
            }

            // Return all application users
            var viewModels = applicationUsers
                .Select(x => new ApplicationUserViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Role = x.Role?.Id ?? Guid.Empty,
                    RoleName = x.Role?.Name
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

            // Find the specified application user
            var viewModel = await _context.ApplicationUsers
                // Include role
                .Include(x => x.Role)
                // Ensure the application user has not been deleted
                .Where(x => !x.Disabled)
                .Select(x => new ApplicationUserViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Role = x.Role.Id,
                    RoleName = x.Role.Name
                })
                .FirstOrDefaultAsync(x => x.UserId == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown application user {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access application user {id}");
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
        public async Task<IActionResult> PutApplicationUser(Guid id, ApplicationUserViewModel viewModel)
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
            var userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access application user {id}");
                return Forbid();
            }

            // Find the specified application user
            var applicationUser = await _context.ApplicationUsers
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (applicationUser == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown application user {id}");
                return NotFound();
            }

            // Update the application user
            applicationUser.Role = await _context.ApplicationRoles
                .FirstOrDefaultAsync(x => x.Id == viewModel.Role)
                .ConfigureAwait(false);
            applicationUser.UpdatedUser = userId;
            applicationUser.UpdatedTimestamp = DateTimeOffset.UtcNow;

            // Ensure the application role was found
            if (applicationUser.Role == null)
            {
                _logger.LogWarning($"Specified application role {viewModel.Role} could not be found");
                return BadRequest();
            }

            _context.Update(applicationUser);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
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
                Role = await _context.ApplicationRoles
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Role)
                    .ConfigureAwait(false),
                CreatedUser = userId,
                UpdatedUser = userId,
                DisabledUser = userId
            };

            // Ensure the application role was found
            if (applicationUser.Role == null)
            {
                _logger.LogWarning($"Specified application role {viewModel.Role} could not be found");
                return BadRequest();
            }

            await _context.ApplicationUsers.AddAsync(applicationUser)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = applicationUser.Id;

            return CreatedAtAction("GetApplicationUser", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteApplicationUser(Guid id)
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
            var userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to delete application user {id}");
                return Forbid();
            }

            // Find the specified application user
            var applicationUser = await _context.ApplicationUsers
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (applicationUser == null)
            {
                _logger.LogWarning($"User id {userId} attempted to delete unknown application user {id}");
                return NotFound();
            }

            // Disable the application user
            applicationUser.Disabled = true;
            applicationUser.DisabledUser = userId;
            applicationUser.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(applicationUser);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}