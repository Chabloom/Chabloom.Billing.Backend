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
    public class TenantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TenantsController> _logger;
        private readonly IValidator _validator;

        public TenantsController(ApplicationDbContext context, ILogger<TenantsController> logger, IValidator validator)
        {
            _context = context;
            _logger = logger;
            _validator = validator;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<TenantViewModel>>> GetTenants()
        {
            // Find all tenants
            var tenants = await _context.Tenants
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);

            // Convert to view models
            var viewModels = tenants
                .Select(x => new TenantViewModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToList();

            return Ok(viewModels);
        }

        [HttpGet("Authorized")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<TenantViewModel>>> GetTenantsAuthorized()
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

            // Find all tenants
            var tenants = await _context.Tenants
                // Include the users
                .Include(x => x.Users)
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (!tenants.Any())
            {
                return new List<TenantViewModel>();
            }

            var authorizedTenants = tenants;

            // Check if the user is authorized at the application level
            var userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                // Filter tenants by user id
                authorizedTenants = tenants
                    .Where(x => x.Users.Select(y => y.UserId).Contains(userId))
                    .ToList();
                if (!authorizedTenants.Any())
                {
                    return new List<TenantViewModel>();
                }
            }

            // Convert to view models
            var viewModels = authorizedTenants
                .Select(x => new TenantViewModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToList();

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TenantViewModel>> GetTenant(Guid id)
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

            // Find the specified tenant
            var viewModel = await _context.Tenants
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .Select(x => new TenantViewModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown tenant {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access tenant {id}");
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
        public async Task<IActionResult> PutTenant(Guid id, TenantViewModel viewModel)
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
            var sid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

            // Find the specified tenant if the user has access to it
            var tenant = await _context.Tenants
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (tenant == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown tenant {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to update tenant {id}");
                return Forbid();
            }

            // Update the tenant
            tenant.Name = viewModel.Name;
            tenant.UpdatedUser = userId;
            tenant.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(tenant);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            _logger.LogInformation($"User {userId} updated tenant {id}");

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TenantViewModel>> PostTenant(TenantViewModel viewModel)
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
            var sid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
            var userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to create tenants");
                return Forbid();
            }

            // Create the new tenant
            var tenant = new Tenant
            {
                Name = viewModel.Name,
                CreatedUser = userId
            };

            await _context.Tenants.AddAsync(tenant)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = tenant.Id;

            _logger.LogInformation($"User {userId} created tenant {tenant.Id}");

            return CreatedAtAction("GetTenant", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteTenant(Guid id)
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

            // Find the specified tenant if the user has access to it
            var tenant = await _context.Tenants
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (tenant == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown tenant {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckApplicationAccessAsync(userId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to delete tenants");
                return Forbid();
            }

            // Disable the tenant
            tenant.Disabled = true;
            tenant.DisabledUser = userId;
            tenant.DisabledTimestamp = DateTimeOffset.UtcNow;

            _context.Update(tenant);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}