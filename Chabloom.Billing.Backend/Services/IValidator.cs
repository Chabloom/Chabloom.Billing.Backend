// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Chabloom.Billing.Backend.Services
{
    public interface IValidator
    {
        /// <summary>
        ///     Get the tenant roles the user has access to
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <param name="tenantId">The tenant id</param>
        /// <returns>A list of all roles the user has access to</returns>
        public Task<List<string>> GetTenantRolesAsync(Guid userId, Guid tenantId);

        /// <summary>
        ///     Validate that a user has a user id
        /// </summary>
        /// <param name="user">The claims principal to check</param>
        /// <returns>The user id or an empty guid on failure</returns>
        public Guid GetUserId(ClaimsPrincipal user);
    }
}