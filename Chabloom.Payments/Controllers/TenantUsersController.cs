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
    public class TenantUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TenantUsersController> _logger;
        private readonly IValidator _validator;

        public TenantUsersController(ApplicationDbContext context, ILogger<TenantUsersController> logger,
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
        public async Task<ActionResult<IEnumerable<TenantUserViewModel>>> GetTenantUsers(Guid tenantId)
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

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, tenantId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access tenant users");
                return Forbid();
            }

            // Get all tenant users the user is authorized to view
            var tenantUsers = await _context.TenantUsers
                // For the specified tenant
                .Where(x => x.Tenant.Id == tenantId)
                .ToListAsync()
                .ConfigureAwait(false);
            if (!tenantUsers.Any())
            {
                return new List<TenantUserViewModel>();
            }

            // Convert to view models
            var viewModels = tenantUsers
                .Select(x => new TenantUserViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    TenantId = x.TenantId
                })
                .Distinct()
                .ToList();

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TenantUserViewModel>> GetTenantUser(Guid id)
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

            // Find the specified tenant user
            var viewModel = await _context.TenantUsers
                .Select(x => new TenantUserViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    TenantId = x.TenantId
                })
                .FirstOrDefaultAsync(x => x.UserId == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown tenant user {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, viewModel.TenantId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access tenant user {id}");
                return Forbid();
            }

            return Ok(viewModel);
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TenantUserViewModel>> PostTenantUser(TenantUserViewModel viewModel)
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
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, viewModel.Id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to create tenant users");
                return Forbid();
            }

            // Find the specified tenant
            var tenant = await _context.Tenants
                .FindAsync(viewModel.TenantId)
                .ConfigureAwait(false);
            if (tenant == null)
            {
                _logger.LogWarning($"Specified tenant {viewModel.TenantId} could not be found");
                return BadRequest();
            }

            // Create the new tenant user
            var tenantUser = new TenantUser
            {
                UserId = viewModel.UserId,
                Tenant = tenant,
                CreatedUser = userId
            };

            await _context.TenantUsers.AddAsync(tenantUser)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = tenantUser.Id;

            _logger.LogInformation($"User {userId} added {viewModel.UserId} to tenant {tenant.Id}");

            return CreatedAtAction("GetTenantUser", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteTenantUser(Guid id)
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

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to delete tenant user {id}");
                return Forbid();
            }

            // Find the specified tenant user
            var tenantUser = await _context.TenantUsers
                .FindAsync(id)
                .ConfigureAwait(false);
            if (tenantUser == null)
            {
                _logger.LogWarning($"User id {userId} attempted to delete unknown tenant user {id}");
                return NotFound();
            }

            _context.Remove(tenantUser);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            _logger.LogInformation($"User {userId} removed {tenantUser.UserId} from tenant {tenantUser.TenantId}");

            return NoContent();
        }
    }
}