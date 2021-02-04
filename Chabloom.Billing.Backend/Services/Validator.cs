// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
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

        public async Task<bool> CheckAccountAccessAsync(Guid userId, Guid accountId)
        {
            // Lookup the account
            var account = await _context.Accounts
                .Include(x => x.Users)
                .Include(x => x.Tenant)
                .ThenInclude(x => x.Users)
                .FirstOrDefaultAsync(x => x.Id == accountId)
                .ConfigureAwait(false);
            if (account != null)
            {
                // Check if the user exists at the account level
                var accountUsers = account.Users
                    .Select(x => x.UserId)
                    .ToList();
                if (accountUsers.Contains(userId))
                {
                    return true;
                }

                // Check if the user exists at the tenant level
                var tenantUsers = account.Tenant.Users
                    .Select(x => x.UserId)
                    .ToList();
                if (tenantUsers.Contains(userId))
                {
                    return true;
                }
            }

            // Check if the user exists at the application level
            var applicationUsers = await _context.ApplicationUsers
                .Select(x => x.UserId)
                .ToListAsync()
                .ConfigureAwait(false);
            if (applicationUsers.Contains(userId))
            {
                return true;
            }

            _logger.LogWarning($"Denied account-level access to user id {userId}");
            return false;
        }

        public async Task<bool> CheckTenantAccessAsync(Guid userId, Guid tenantId)
        {
            // Lookup the tenant
            var tenant = await _context.Tenants
                .Include(x => x.Users)
                .FirstOrDefaultAsync(x => x.Id == tenantId)
                .ConfigureAwait(false);
            if (tenant != null)
            {
                // Check if the user exists at the tenant level
                var tenantUsers = tenant.Users
                    .Select(x => x.UserId)
                    .ToList();
                if (tenantUsers.Contains(userId))
                {
                    return true;
                }
            }

            // Check if the user exists at the application level
            var applicationUsers = await _context.ApplicationUsers
                .Select(x => x.UserId)
                .ToListAsync()
                .ConfigureAwait(false);
            if (applicationUsers.Contains(userId))
            {
                return true;
            }

            _logger.LogWarning($"Denied tenant-level access to user id {userId}");
            return false;
        }

        public async Task<bool> CheckApplicationAccessAsync(Guid userId)
        {
            // Check if the user exists at the application level
            var applicationUsers = await _context.ApplicationUsers
                .Select(x => x.UserId)
                .ToListAsync()
                .ConfigureAwait(false);
            if (applicationUsers.Contains(userId))
            {
                return true;
            }

            _logger.LogWarning($"Denied application-level access to user id {userId}");
            return false;
        }

        public Guid GetUserId(ClaimsPrincipal user)
        {
            // Get the current user sid
            var sid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("User attempted call without an sid");
                return Guid.Empty;
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"User sid {sid} could not be parsed as Guid");
                return Guid.Empty;
            }

            return userId;
        }
    }
}