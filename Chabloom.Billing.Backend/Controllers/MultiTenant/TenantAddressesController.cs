// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Models.MultiTenant;
using Chabloom.Billing.Backend.Services;
using Chabloom.Billing.Backend.ViewModels.MultiTenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Billing.Backend.Controllers.MultiTenant
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TenantAddressesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TenantAddressesController> _logger;
        private readonly IValidator _validator;

        public TenantAddressesController(ApplicationDbContext context, ILogger<TenantAddressesController> logger,
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
        public async Task<IActionResult> GetTenantAddressesAsync([FromQuery] Guid tenantId)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            var userRoles = await _validator.GetTenantRolesAsync(userId, tenantId);
            if (!userRoles.Contains("Admin") && !userRoles.Contains("Manager"))
            {
                _logger.LogWarning($"User id {userId} was not authorized to read addresses for tenant {tenantId}");
                return Forbid();
            }

            // Get all tenant addresses for the tenant
            var tenantAddresses = await _context.TenantAddresses
                .Where(x => x.TenantId == tenantId)
                .ToListAsync();
            if (tenantAddresses == null)
            {
                return Ok(new List<TenantAddressViewModel>());
            }

            // Convert to view models
            var viewModels = tenantAddresses
                .Select(x => new TenantAddressViewModel
                {
                    Address = x.Address,
                    TenantId = x.TenantId
                })
                .ToList();

            return Ok(viewModels);
        }

        [HttpPost("Create")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> CreateTenantAddressAsync([FromBody] TenantAddressViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            var userRoles = await _validator.GetTenantRolesAsync(userId, viewModel.TenantId);
            if (!userRoles.Contains("Admin") && !userRoles.Contains("Manager"))
            {
                _logger.LogWarning(
                    $"User id {userId} was not authorized to create addresses for tenant {viewModel.TenantId}");
                return Forbid();
            }

            // Ensure the tenant address does not yet exist
            var tenantAddress = await _context.TenantAddresses
                .Where(x => x.TenantId == viewModel.TenantId)
                .FirstOrDefaultAsync(x => x.Address == viewModel.Address);
            if (tenantAddress != null)
            {
                return Conflict();
            }

            // Create the new tenant address
            tenantAddress = new TenantAddress
            {
                Address = viewModel.Address,
                TenantId = viewModel.TenantId
            };

            await _context.AddAsync(tenantAddress);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                $"User {userId} added tenant address {viewModel.Address} to tenant {viewModel.TenantId}");

            return Ok();
        }

        [HttpPost("Delete")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteTenantAddressAsync([FromBody] TenantAddressViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            var userRoles = await _validator.GetTenantRolesAsync(userId, viewModel.TenantId);
            if (!userRoles.Contains("Admin") && !userRoles.Contains("Manager"))
            {
                _logger.LogWarning(
                    $"User id {userId} was not authorized to delete addresses for tenant {viewModel.TenantId}");
                return Forbid();
            }

            // Find the specified tenant address
            var tenantAddress = await _context.TenantAddresses
                .Where(x => x.TenantId == viewModel.TenantId)
                .FirstOrDefaultAsync(x => x.Address == viewModel.Address);
            if (tenantAddress == null)
            {
                return NotFound();
            }

            _context.Remove(tenantAddress);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                $"User {userId} removed tenant address {viewModel.Address} from tenant {viewModel.TenantId}");

            return Ok();
        }
    }
}