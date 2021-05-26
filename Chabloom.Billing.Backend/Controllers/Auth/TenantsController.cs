// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Models.Auth;
using Chabloom.Billing.Backend.Services;
using Chabloom.Billing.Backend.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Billing.Backend.Controllers.Auth
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
                .ToListAsync();

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
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find all tenants
            var tenants = new List<Tenant>();
            var userRoles = await _context.UserRoles
                .Where(x => x.UserId == userId)
                .ToListAsync();
            foreach (var userRole in userRoles)
            {
                var role = await _context.Roles
                    .FirstOrDefaultAsync(x => x.Id == userRole.RoleId);
                if (role == null)
                {
                    continue;
                }

                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(x => x.Id == role.TenantId);
                if (tenant != null)
                {
                    tenants.Add(tenant);
                }
            }

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

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TenantViewModel>> GetTenant(Guid id)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified tenant
            var viewModel = await _context.Tenants
                .Select(x => new TenantViewModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .FirstOrDefaultAsync(x => x.Id == id);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown tenant {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userRoles = await _validator.GetTenantRolesAsync(userId, id);
            if (!userRoles.Contains("Admin") && !userRoles.Contains("Manager"))
            {
                _logger.LogWarning($"User id {userId} was not authorized to access tenant {id}");
                return Forbid();
            }

            return Ok(viewModel);
        }
    }
}