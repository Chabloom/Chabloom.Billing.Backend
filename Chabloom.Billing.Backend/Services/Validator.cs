// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Chabloom.Billing.Backend.Services
{
    public class Validator : IValidator
    {
        private readonly ApplicationDbContext _context;

        public Validator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetTenantRolesAsync(Guid userId, Guid tenantId)
        {
            // Attempt to find the user in the tenant users
            var user = await _context.Users
                .Where(x => x.TenantId == tenantId)
                .FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return new List<string>();
            }

            // Get the name of all user roles
            var userRoles = await _context.UserRoles
                .Join(
                    _context.Roles,
                    userRole => userRole.RoleId,
                    role => role.Id,
                    (userRole, role) => role)
                .Where(x => x.TenantId == tenantId)
                .Select(x => x.Name)
                .ToListAsync();

            return userRoles;
        }

        public Guid GetUserId(ClaimsPrincipal user)
        {
            // Get the current user sid
            var sid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sid))
            {
                return Guid.Empty;
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                return Guid.Empty;
            }

            return userId;
        }
    }
}