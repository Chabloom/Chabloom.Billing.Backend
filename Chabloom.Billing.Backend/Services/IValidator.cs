// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Chabloom.Billing.Backend.Services
{
    public interface IValidator
    {
        /// <summary>
        ///     Get a user id based on a claims principal
        /// </summary>
        /// <param name="user">The claims principal to check</param>
        /// <returns>The user id or null on failure</returns>
        public Guid? GetUserId(ClaimsPrincipal user);

        /// <summary>
        ///     Get the tenant id associated with an http request
        /// </summary>
        /// <param name="request">The request passed to the endpoint controller</param>
        /// <returns>The tenant or null on failure</returns>
        public Task<Guid?> GetTenantIdAsync(HttpRequest request);

        /// <summary>
        ///     Get the tenant roles the user has access to
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <param name="tenantId">The tenant id</param>
        /// <returns>A list of all roles the user has access to</returns>
        public Task<IList<string>> GetTenantRolesAsync(Guid userId, Guid tenantId);
    }
}