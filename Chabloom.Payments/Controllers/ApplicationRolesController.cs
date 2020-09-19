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
    public class ApplicationRolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountRolesController> _logger;
        private readonly IValidator _validator;

        public ApplicationRolesController(ApplicationDbContext context, ILogger<AccountRolesController> logger,
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
        public async Task<ActionResult<IEnumerable<ApplicationRoleViewModel>>> GetApplicationRoles()
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("Role attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"Role sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"Role id {userId} was not authorized to access application roles");
                return Forbid();
            }

            // Get all application roles
            var applicationRoles = await _context.ApplicationRoles
                // Ensure the application role has not been deleted
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (applicationRoles == null || !applicationRoles.Any())
            {
                return new List<ApplicationRoleViewModel>();
            }

            // Return all application roles
            var viewModels = applicationRoles
                .Select(x => new ApplicationRoleViewModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .Distinct()
                .ToList();

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApplicationRoleViewModel>> GetApplicationRole(Guid id)
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("Role attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"Role sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // Find the specified application role
            var viewModel = await _context.ApplicationRoles
                // Ensure the application role has not been deleted
                .Where(x => !x.Disabled)
                .Select(x => new ApplicationRoleViewModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"Role id {userId} attempted to access unknown application role {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"Role id {userId} was not authorized to access application role {id}");
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
        public async Task<IActionResult> PutApplicationRole(Guid id, ApplicationRoleViewModel viewModel)
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
                _logger.LogWarning($"User id {userId} was not authorized to access application role {id}");
                return Forbid();
            }

            // Find the specified application role
            var applicationRole = await _context.ApplicationRoles
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (applicationRole == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown application role {id}");
                return NotFound();
            }

            // Update the application role
            applicationRole.Name = viewModel.Name;
            applicationRole.UpdatedUser = userId;
            applicationRole.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(applicationRole);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApplicationRoleViewModel>> PostApplicationRole(
            ApplicationRoleViewModel viewModel)
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
                _logger.LogWarning($"User id {userId} was not authorized to create application roles");
                return Forbid();
            }

            // Create the new application role
            var applicationRole = new ApplicationRole
            {
                Name = viewModel.Name,
                CreatedUser = userId,
                UpdatedUser = userId,
                DisabledUser = userId
            };

            await _context.ApplicationRoles.AddAsync(applicationRole)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = applicationRole.Id;

            return CreatedAtAction("GetApplicationRole", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteApplicationRole(Guid id)
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
                _logger.LogWarning($"User id {userId} was not authorized to delete application role {id}");
                return Forbid();
            }

            // Find the specified application role
            var applicationRole = await _context.ApplicationRoles
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (applicationRole == null)
            {
                _logger.LogWarning($"User id {userId} attempted to delete unknown application role {id}");
                return NotFound();
            }

            // Disable the application role
            applicationRole.Disabled = true;
            applicationRole.DisabledUser = userId;
            applicationRole.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(applicationRole);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}