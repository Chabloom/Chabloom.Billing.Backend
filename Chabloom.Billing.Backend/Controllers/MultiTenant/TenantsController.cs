// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Services;
using Chabloom.Billing.Backend.ViewModels.MultiTenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Billing.Backend.Controllers.MultiTenant
{
    [AllowAnonymous]
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

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTenantAsync(Guid id)
        {
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
                return NotFound();
            }

            return Ok(viewModel);
        }

        [HttpGet("Current")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCurrentTenantAsync()
        {
            // Get the current tenant
            var tenant = await _validator.GetCurrentTenantAsync(Request);
            if (tenant == null)
            {
                _logger.LogWarning($"Could not find tenant for address");
                return NotFound();
            }

            var retViewModel = new TenantViewModel
            {
                Id = tenant.Id,
                Name = tenant.Name
            };

            return Ok(retViewModel);
        }
    }
}