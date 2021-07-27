// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Models.Tenants;
using Chabloom.Billing.Backend.Services;
using Chabloom.Billing.Backend.ViewModels.Tenants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Billing.Backend.Controllers.Tenants
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TenantHostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TenantHostsController> _logger;
        private readonly IValidator _validator;

        public TenantHostsController(ApplicationDbContext context, ILogger<TenantHostsController> logger,
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
        public async Task<IActionResult> GetTenantHostsAsync([FromQuery] Guid tenantId)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (!userId.HasValue)
            {
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            var (roleValid, roleResult) = await ValidateRoleAccessAsync(userId.Value, tenantId);
            if (!roleValid)
            {
                return roleResult;
            }

            // Get all tenant hosts for the tenant
            var tenantHosts = await _context.TenantHosts
                .Where(x => x.TenantId == tenantId)
                .ToListAsync();
            if (tenantHosts == null)
            {
                return Ok(new List<TenantHostViewModel>());
            }

            // Convert to view models
            var viewModels = tenantHosts
                .Select(x => new TenantHostViewModel
                {
                    Hostname = x.Hostname,
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
        public async Task<IActionResult> CreateTenantHostAsync([FromBody] TenantHostViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user id
            var userId = _validator.GetUserId(User);
            if (!userId.HasValue)
            {
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            var (roleValid, roleResult) = await ValidateRoleAccessAsync(userId.Value, viewModel.TenantId);
            if (!roleValid)
            {
                return roleResult;
            }

            // Ensure the tenant host does not yet exist
            var tenantHost = await _context.TenantHosts
                .Where(x => x.TenantId == viewModel.TenantId)
                .FirstOrDefaultAsync(x => x.Hostname == viewModel.Hostname);
            if (tenantHost != null)
            {
                return Conflict();
            }

            // Create the new tenant host
            tenantHost = new TenantHost
            {
                Hostname = viewModel.Hostname,
                TenantId = viewModel.TenantId
            };

            await _context.AddAsync(tenantHost);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                $"User {userId} added tenant host {viewModel.Hostname} to tenant {viewModel.TenantId}");

            return Ok();
        }

        [HttpPost("Delete")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteTenantHostAsync([FromBody] TenantHostViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user id
            var userId = _validator.GetUserId(User);
            if (!userId.HasValue)
            {
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            var (roleValid, roleResult) = await ValidateRoleAccessAsync(userId.Value, viewModel.TenantId);
            if (!roleValid)
            {
                return roleResult;
            }

            // Find the specified tenant host
            var tenantHost = await _context.TenantHosts
                .Where(x => x.TenantId == viewModel.TenantId)
                .FirstOrDefaultAsync(x => x.Hostname == viewModel.Hostname);
            if (tenantHost == null)
            {
                return NotFound();
            }

            _context.Remove(tenantHost);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                $"User {userId} removed tenant host {viewModel.Hostname} from tenant {viewModel.TenantId}");

            return Ok();
        }

        private async Task<Tuple<bool, IActionResult>> ValidateRoleAccessAsync(Guid userId, Guid tenantId)
        {
            // Ensure the user is authorized at the requested level
            var userRoles = await _validator.GetTenantRolesAsync(userId, tenantId);
            if (!userRoles.Contains("Admin") &&
                !userRoles.Contains("Manager"))
            {
                _logger.LogWarning($"User id {userId} was not authorized to perform requested operation");
                return new Tuple<bool, IActionResult>(false, Forbid());
            }

            return new Tuple<bool, IActionResult>(true, Ok());
        }
    }
}