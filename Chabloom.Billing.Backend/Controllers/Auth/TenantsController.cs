// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Billing.Backend.Controllers.Auth
{
    [AllowAnonymous]
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

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTenant(Guid id)
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
        public async Task<IActionResult> GetCurrentTenant()
        {
            // Get the referrer address based on the header value
            var headers = Request.GetTypedHeaders();
            var referrer = headers.Referer;
            if (referrer == null)
            {
                return BadRequest();
            }
            var address = referrer.AbsoluteUri;
            if (string.IsNullOrEmpty(address))
            {
                return BadRequest();
            }

            // Find the tenant belonging to the specified address
            var tenantAddress = await _context.TenantAddresses
                .Include(x => x.Tenant)
                // ReSharper disable once SpecifyStringComparison
                .FirstOrDefaultAsync(x => x.Address.ToUpper() == address.ToUpper());
            if (tenantAddress == null)
            {
                _logger.LogError($"Could not find tenant for address {address}");
                return NotFound();
            }

            var retViewModel = new TenantViewModel
            {
                Id = tenantAddress.Tenant.Id,
                Name = tenantAddress.Tenant.Name
            };

            return Ok(retViewModel);
        }
    }
}