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
        public async Task<ActionResult<IEnumerable<TenantUserViewModel>>> GetTenantUsers(Guid? tenantId)
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
                _logger.LogWarning($"User id {userId} was not authorized to access tenant users");
                return Forbid();
            }

            // Get all tenant users the user is authorized to view
            var tenantUsers = await _context.TenantUsers
                // Include the tenant
                .Include(x => x.Tenant)
                // Include the role
                .Include(x => x.Role)
                // Ensure the tenant user has not been deleted
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (tenantUsers == null || !tenantUsers.Any())
            {
                return new List<TenantUserViewModel>();
            }

            List<TenantUserViewModel> viewModels;
            if (tenantId != null)
            {
                // Filter account users by tenant id
                viewModels = tenantUsers
                    .Where(x => x.Tenant.Id == tenantId)
                    .Select(x => new TenantUserViewModel
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        Tenant = x.Tenant.Id,
                        Role = x.Role.Id,
                        RoleName = x.Role.Name
                    })
                    .ToList();
            }
            else
            {
                // Do not filter users
                viewModels = tenantUsers
                    .Select(x => new TenantUserViewModel
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        Tenant = x.Tenant.Id,
                        Role = x.Role.Id,
                        RoleName = x.Role.Name
                    })
                    .ToList();
            }

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TenantUserViewModel>> GetTenantUser(Guid id)
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

            // Find the specified tenant user
            var viewModel = await _context.TenantUsers
                // Include the tenant
                .Include(x => x.Tenant)
                // Include the role
                .Include(x => x.Role)
                // Ensure the tenant user has not been deleted
                .Where(x => !x.Disabled)
                .Select(x => new TenantUserViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Tenant = x.Tenant.Id,
                    Role = x.Role.Id,
                    RoleName = x.Role.Name
                })
                .FirstOrDefaultAsync(x => x.UserId == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown tenant user {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, viewModel.Tenant)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access tenant user {id}");
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
        public async Task<IActionResult> PutTenantUser(Guid id, TenantUserViewModel viewModel)
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

            // Find the specified tenantUser
            var tenantUser = await _context.TenantUsers
                .Include(x => x.Tenant)
                .ThenInclude(x => x.Users)
                .ThenInclude(x => x.Role)
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (tenantUser == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown tenant user {id}");
                return NotFound();
            }

            // Ensure the current user belongs to the tenant
            if (!tenantUser.Tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {tenantUser.Tenant.Id}");
                return Forbid();
            }

            // Ensure the current user is a tenant admin or manager
            var curTenantUser = tenantUser.Tenant.Users
                .FirstOrDefault(x => x.UserId == userId);
            if (curTenantUser != null &&
                curTenantUser.Role.Name != "Admin" &&
                curTenantUser.Role.Name != "Manager")
            {
                _logger.LogWarning(
                    $"User id {userId} did not have permissions to update tenant user for tenant {tenantUser.Tenant.Id}");
                return Forbid();
            }

            // Update the tenant user
            tenantUser.Role = await _context.TenantRoles
                .FirstOrDefaultAsync(x => x.Id == viewModel.Role)
                .ConfigureAwait(false);
            tenantUser.UpdatedUser = userId;
            tenantUser.UpdatedTimestamp = DateTimeOffset.UtcNow;

            // Ensure the tenant role was found
            if (tenantUser.Role == null)
            {
                _logger.LogWarning($"Specified tenant role {viewModel.Role} could not be found");
                return BadRequest();
            }

            _context.Update(tenantUser);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
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

            // Create the new tenant user
            var tenantUser = new TenantUser
            {
                UserId = userId,
                Tenant = await _context.Tenants
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Tenant)
                    .ConfigureAwait(false),
                Role = await _context.TenantRoles
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Role)
                    .ConfigureAwait(false),
                CreatedUser = userId,
                UpdatedUser = userId,
                DisabledUser = userId
            };

            // Ensure the tenant was found
            if (tenantUser.Tenant == null)
            {
                _logger.LogWarning($"Specified tenant {viewModel.Tenant} could not be found");
                return BadRequest();
            }

            // Ensure the tenant role was found
            if (tenantUser.Role == null)
            {
                _logger.LogWarning($"Specified tenant role {viewModel.Role} could not be found");
                return BadRequest();
            }

            // Ensure the current user belongs to the tenant
            if (!tenantUser.Tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {tenantUser.Tenant.Id}");
                return Forbid();
            }

            // Ensure the current user is a tenant admin or manager
            var curTenantUser = tenantUser.Tenant.Users
                .FirstOrDefault(x => x.UserId == userId);
            if (curTenantUser != null &&
                curTenantUser.Role.Name != "Admin" &&
                curTenantUser.Role.Name != "Manager")
            {
                _logger.LogWarning(
                    $"User id {userId} did not have permissions to create tenant user for tenant {tenantUser.Tenant.Id}");
                return Forbid();
            }

            await _context.TenantUsers.AddAsync(tenantUser)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = tenantUser.Id;

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

            // Find the specified account if the user has access to it
            var tenantUser = await _context.TenantUsers
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (tenantUser == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown account {id}");
                return NotFound();
            }

            // Ensure the current user belongs to the tenant
            if (!tenantUser.Tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {tenantUser.Tenant.Id}");
                return Forbid();
            }

            // Ensure the current user is a tenant admin or manager
            var curTenantUser = tenantUser.Tenant.Users
                .FirstOrDefault(x => x.UserId == userId);
            if (curTenantUser != null &&
                curTenantUser.Role.Name != "Admin" &&
                curTenantUser.Role.Name != "Manager")
            {
                _logger.LogWarning(
                    $"User id {userId} did not have permissions to disable tenant user for tenant {tenantUser.Tenant.Id}");
                return Forbid();
            }

            // Disable the tenant user
            tenantUser.Disabled = true;
            tenantUser.DisabledUser = userId;
            tenantUser.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(tenantUser);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}