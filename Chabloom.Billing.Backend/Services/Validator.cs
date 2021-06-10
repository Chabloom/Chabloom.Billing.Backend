// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Billing.Backend.Services
{
    public class Validator : IValidator
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<Validator> _logger;

        public Validator(ApplicationDbContext context, ILogger<Validator> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Guid? GetUserId(ClaimsPrincipal user)
        {
            // Get the current user sid
            var sid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sid))
            {
                return null;
            }

            // Ensure the user id can be parsed
            if (Guid.TryParse(sid, out var userId))
            {
                return userId;
            }

            _logger.LogWarning($"Could not parse user sid {sid}");
            return null;
        }

        public async Task<Guid?> GetTenantIdAsync(HttpRequest request)
        {
            // Get the referer address
            var address = request.GetTypedHeaders()?.Referer?.Authority;
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            // Find the tenant host for the referer address
            var tenantAddress = await _context.TenantHosts
                .FirstOrDefaultAsync(x => x.Hostname.ToUpper() == address.ToUpper());
            if (tenantAddress != null)
            {
                return tenantAddress.TenantId;
            }

            _logger.LogWarning($"Could not find tenant for address {address}");
            return null;
        }

        public async Task<IList<string>> GetTenantRolesAsync(Guid userId, Guid tenantId)
        {
            // Attempt to find the user in the tenant users
            var user = await _context.Users
                .Where(x => x.TenantId == tenantId)
                .FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return new List<string>();
            }

            // Get all user roles
            var roleNames = await _context.UserRoles
                .Join(
                    _context.Roles,
                    userRole => userRole.RoleId,
                    role => role.Id,
                    (userRole, role) => role)
                .Where(x => x.TenantId == tenantId)
                .Select(x => x.Name)
                .ToListAsync();

            return roleNames;
        }
    }
}