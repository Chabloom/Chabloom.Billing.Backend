// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chabloom.Payments.Data;
using Chabloom.Payments.Models;
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

        public TenantsController(ApplicationDbContext context, ILogger<TenantsController> logger)
        {
            _context = context;
            _logger = logger;
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
                .Include(x => x.Users)
                .Where(x => !x.Disabled)
                .Select(x => new TenantViewModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return Ok(tenants);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TenantViewModel>> GetTenant(Guid id)
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

            // Find the specified tenant if the user has access to it
            var tenant = await _context.Tenants
                .Include(x => x.Users)
                .Where(x => x.Users.Select(y => y.UserId).Contains(userId))
                .Where(x => !x.Disabled)
                .Select(x => new TenantViewModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (tenant == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown tenant {id}");
                return NotFound();
            }

            return Ok(tenant);
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

            // Find the specified tenant if the user has access to it
            var tenant = await _context.Tenants
                .Include(x => x.Users)
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (tenant == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown tenant {id}");
                return NotFound();
            }

            // Ensure the current user belongs to the tenant
            if (!tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {tenant.Id}");
                return Forbid();
            }

            // Update the tenant
            tenant.Name = viewModel.Name;
            tenant.UpdatedUser = userId;
            tenant.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(tenant);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

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

            // Create the new tenant
            var tenant = new Tenant
            {
                Name = viewModel.Name,
                CreatedUser = userId,
                UpdatedUser = userId,
                DisabledUser = userId
            };

            // Ensure the current user can create tenants
            var applicationUser = _context.ApplicationUsers
                .Include(x => x.Role)
                .FirstOrDefault(x => x.UserId == userId);
            if (applicationUser != null &&
                applicationUser.Role.Name != "Admin" &&
                applicationUser.Role.Name != "Manager")
            {
                _logger.LogWarning($"User id {userId} did not have permissions to create tenants");
                return Forbid();
            }

            await _context.Tenants.AddAsync(tenant)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Create the tenant roles
            var roles = new List<TenantRole>
            {
                new TenantRole
                {
                    Name = "Admin", 
                    Tenant = tenant,
                    CreatedUser = userId,
                    UpdatedUser = userId,
                    DisabledUser = userId
                },
                new TenantRole
                {
                    Name = "Manager", 
                    Tenant = tenant,
                    CreatedUser = userId,
                    UpdatedUser = userId,
                    DisabledUser = userId
                },
                new TenantRole
                {
                    Name = "Basic", 
                    Tenant = tenant,
                    CreatedUser = userId,
                    UpdatedUser = userId,
                    DisabledUser = userId
                }
            };

            await _context.AddRangeAsync(roles)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Add the user to the new tenant as an admin
            var tenantUser = new TenantUser
            {
                UserId = userId,
                Tenant = tenant,
                Role = await _context.TenantRoles
                    .Where(x => x.Tenant == tenant)
                    .FirstOrDefaultAsync(x => x.Name == "Admin")
                    .ConfigureAwait(false),
                CreatedUser = userId,
                UpdatedUser = userId,
                DisabledUser = userId
            };

            await _context.TenantUsers.AddAsync(tenantUser)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = tenant.Id;

            return CreatedAtAction("GetTenant", new {id = viewModel.Id}, viewModel);
        }
    }
}