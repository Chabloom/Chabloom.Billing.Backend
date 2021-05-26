// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Services;
using Chabloom.Billing.Backend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chabloom.Billing.Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserRolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator _validator;

        public UserRolesController(ApplicationDbContext context, IValidator validator)
        {
            _context = context;
            _validator = validator;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<UserRoleViewModel>>> GetUserRoles([FromQuery] Guid tenantId)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Get all roles the user belongs to
            var userRoles = await _context.UserRoles
                .Where(x => x.UserId == userId)
                .Select(x => x.RoleId)
                .ToListAsync();
            if (userRoles == null)
            {
                return new List<UserRoleViewModel>();
            }

            // Get the actual role objects
            var roles = await _context.Roles
                .Where(x => x.TenantId == tenantId)
                .Where(x => userRoles.Contains(x.Id))
                .ToListAsync();

            // Convert to view models
            var viewModels = roles
                .Select(x => new UserRoleViewModel
                {
                    Name = x.Name,
                    TenantId = x.TenantId
                })
                .ToList();

            return Ok(viewModels);
        }
    }
}