// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chabloom.Payments.Data;
using Chabloom.Payments.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Payments.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApplicationUsersController> _logger;

        public ApplicationUsersController(ApplicationDbContext context, ILogger<ApplicationUsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<ApplicationUserViewModel>>> GetApplicationUsers()
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("User attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out _))
            {
                _logger.LogWarning($"User sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // Find all application users
            var applicationUsers = await _context.ApplicationUsers
                .Include(x => x.Role)
                .ThenInclude(x => x.Users)
                .Where(x => !x.Disabled)
                .Select(x => new ApplicationUserViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Role = x.Role.Name,
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return Ok(applicationUsers);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ApplicationUserViewModel>> GetApplicationUser(Guid id)
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

            // Find the specified application user if the user has access to it
            var applicationUser = await _context.ApplicationUsers
                .Include(x => x.Role)
                .ThenInclude(x => x.Users)
                .Where(x => !x.Disabled)
                .Select(x => new ApplicationUserViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Role = x.Role.Name,
                })
                .FirstOrDefaultAsync(x => x.UserId == id)
                .ConfigureAwait(false);
            if (applicationUser == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown application user {id}");
                return NotFound();
            }

            return Ok(applicationUser);
        }
    }
}