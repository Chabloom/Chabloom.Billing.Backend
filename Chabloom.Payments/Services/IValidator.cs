// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Threading.Tasks;

namespace Chabloom.Payments.Services
{
    public interface IValidator
    {
        /// <summary>
        ///     Determine if a specified user has access at the account level or higher
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <param name="accountId">The account id</param>
        /// <returns>True if the user is authorized, false otherwise</returns>
        public Task<bool> CheckAccountAccessAsync(Guid userId, Guid accountId);

        /// <summary>
        ///     Determine if a specified user has access at the tenant level or higher
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <param name="tenantId">The tenant id</param>
        /// <returns>True if the user is authorized, false otherwise</returns>
        public Task<bool> CheckTenantAccessAsync(Guid userId, Guid tenantId);

        /// <summary>
        ///     Determine if a specified user has access at the application level
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <returns>True if the user is authorized, false otherwise</returns>
        public Task<bool> CheckApplicationAccessAsync(Guid userId);
    }
}