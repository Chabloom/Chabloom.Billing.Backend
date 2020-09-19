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
    public class TenantRolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountRolesController> _logger;
        private readonly IValidator _validator;

        public TenantRolesController(ApplicationDbContext context, ILogger<AccountRolesController> logger,
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
        public async Task<ActionResult<IEnumerable<TenantRoleViewModel>>> GetTenantRoles(Guid? tenantId)
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
                _logger.LogWarning($"Role id {userId} was not authorized to access tenant roles");
                return Forbid();
            }

            // Get all tenant roles the role is authorized to view
            var tenantRoles = await _context.TenantRoles
                // Include the tenant
                .Include(x => x.Tenant)
                // Ensure the tenant role has not been deleted
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (tenantRoles == null || !tenantRoles.Any())
            {
                return new List<TenantRoleViewModel>();
            }

            List<TenantRoleViewModel> viewModels;
            if (tenantId != null)
            {
                // Filter account roles by tenant id
                viewModels = tenantRoles
                    .Where(x => x.Tenant.Id == tenantId)
                    .Select(x => new TenantRoleViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Tenant = x.Tenant.Id
                    })
                    .Distinct()
                    .ToList();
            }
            else
            {
                // Do not filter roles
                viewModels = tenantRoles
                    .Select(x => new TenantRoleViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Tenant = x.Tenant.Id
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
        public async Task<ActionResult<TenantRoleViewModel>> GetTenantRole(Guid id)
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

            // Find the specified tenant role
            var viewModel = await _context.TenantRoles
                // Include the tenant
                .Include(x => x.Tenant)
                // Ensure the tenant role has not been deleted
                .Where(x => !x.Disabled)
                .Select(x => new TenantRoleViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Tenant = x.Tenant.Id
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"Role id {userId} attempted to access unknown tenant role {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, viewModel.Tenant)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"Role id {userId} was not authorized to access tenant role {id}");
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
        public async Task<IActionResult> PutTenantRole(Guid id, TenantRoleViewModel viewModel)
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
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access tenant role {id}");
                return Forbid();
            }

            // Find the specified tenant role
            var tenantRole = await _context.TenantRoles
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (tenantRole == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown tenant role {id}");
                return NotFound();
            }

            // Update the tenant role
            tenantRole.Name = viewModel.Name;
            tenantRole.UpdatedUser = userId;
            tenantRole.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(tenantRole);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TenantRoleViewModel>> PostTenantRole(TenantRoleViewModel viewModel)
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
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, viewModel.Id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to create tenant roles");
                return Forbid();
            }

            // Create the new tenant role
            var tenantRole = new TenantRole
            {
                Name = viewModel.Name,
                Tenant = await _context.Tenants
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Tenant)
                    .ConfigureAwait(false),
                CreatedUser = userId,
                UpdatedUser = userId,
                DisabledUser = userId
            };

            // Ensure the tenant was found
            if (tenantRole.Tenant == null)
            {
                _logger.LogWarning($"Specified tenant {viewModel.Tenant} could not be found");
                return BadRequest();
            }

            await _context.TenantRoles.AddAsync(tenantRole)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = tenantRole.Id;

            return CreatedAtAction("GetTenantRole", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteTenantRole(Guid id)
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
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to delete tenant role {id}");
                return Forbid();
            }

            // Find the specified tenant role
            var tenantRole = await _context.TenantRoles
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (tenantRole == null)
            {
                _logger.LogWarning($"User id {userId} attempted to delete unknown tenant role {id}");
                return NotFound();
            }

            // Disable the tenant role
            tenantRole.Disabled = true;
            tenantRole.DisabledUser = userId;
            tenantRole.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(tenantRole);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}