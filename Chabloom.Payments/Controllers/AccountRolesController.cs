// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chabloom.Payments.Data;
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
    public class AccountRolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountRolesController> _logger;
        private readonly IValidator _validator;

        public AccountRolesController(ApplicationDbContext context, ILogger<AccountRolesController> logger,
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
        public async Task<ActionResult<IEnumerable<AccountRoleViewModel>>> GetAccountRoles(Guid? accountId,
            Guid? tenantId)
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("Role attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"Role sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            bool userAuthorized;
            if (accountId != null)
            {
                userAuthorized = await _validator.CheckAccountAccessAsync(userId, accountId.Value)
                    .ConfigureAwait(false);
            }
            else if (tenantId != null)
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
                _logger.LogWarning($"Role id {userId} was not authorized to access account roles");
                return Forbid();
            }

            // Get all account roles
            var accountRoles = await _context.AccountRoles
                // Include the account and tenant
                .Include(x => x.Account)
                .ThenInclude(x => x.Tenant)
                // Ensure the account role has not been deleted
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (accountRoles == null || !accountRoles.Any())
            {
                return new List<AccountRoleViewModel>();
            }

            List<AccountRoleViewModel> viewModels;
            if (accountId != null)
            {
                // Filter account roles by account id
                viewModels = accountRoles
                    .Where(x => x.Account.Id == accountId)
                    .Select(x => new AccountRoleViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Account = x.Account.Id
                    })
                    .Distinct()
                    .ToList();
            }
            else if (tenantId != null)
            {
                // Filter account roles by tenant id
                viewModels = accountRoles
                    .Where(x => x.Account.Tenant.Id == tenantId)
                    .Select(x => new AccountRoleViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Account = x.Account.Id
                    })
                    .Distinct()
                    .ToList();
            }
            else
            {
                // Do not filter roles
                viewModels = accountRoles
                    .Select(x => new AccountRoleViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Account = x.Account.Id
                    })
                    .Distinct()
                    .ToList();
            }

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<AccountRoleViewModel>> GetAccountRole(Guid id)
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("Role attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"Role sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // Find the specified account role
            var viewModel = await _context.AccountRoles
                // Include the account
                .Include(x => x.Account)
                // Ensure the account role has not been deleted
                .Where(x => !x.Disabled)
                .Select(x => new AccountRoleViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Account = x.Account.Id
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"Role id {userId} attempted to access unknown account role {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            // TODO: Role-Based Access
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, viewModel.Account)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"Role id {userId} was not authorized to access account role {id}");
                return Forbid();
            }

            return Ok(viewModel);
        }
    }
}