// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Services;
using Chabloom.Billing.Backend.ViewModels.Tenants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chabloom.Billing.Backend.Controllers.Tenants
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TenantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator _validator;

        public TenantsController(ApplicationDbContext context, IValidator validator)
        {
            _context = context;
            _validator = validator;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTenantAsync(Guid id)
        {
            // Find the tenant based on the tenant id
            var tenant = await _context.Tenants
                .FindAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }

            var retViewModel = new TenantViewModel
            {
                Id = tenant.Id,
                Name = tenant.Name
            };

            return Ok(retViewModel);
        }

        [AllowAnonymous]
        [HttpGet("Current")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTenantCurrentAsync()
        {
            // Get the current tenant id
            var tenantId = await _validator.GetTenantIdAsync(Request);
            if (!tenantId.HasValue)
            {
                return NotFound();
            }

            // Find the tenant based on the tenant id
            var tenant = await _context.Tenants
                .FindAsync(tenantId);
            if (tenant == null)
            {
                return NotFound();
            }

            var retViewModel = new TenantViewModel
            {
                Id = tenant.Id,
                Name = tenant.Name
            };

            return Ok(retViewModel);
        }

        [HttpGet("Roles")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTenantRolesAsync()
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (!userId.HasValue)
            {
                return Forbid();
            }

            // Get the current tenant id
            var tenantId = await _validator.GetTenantIdAsync(Request);
            if (!tenantId.HasValue)
            {
                return NotFound();
            }

            // Get tenant roles for the user
            var tenantRoles = await _validator.GetTenantRolesAsync(userId.Value, tenantId.Value);

            return Ok(tenantRoles);
        }
    }
}