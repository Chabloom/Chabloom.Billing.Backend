// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using Chabloom.Billing.Backend.Models.Tenants;

namespace Chabloom.Billing.Backend.Models.Accounts
{
    public class UserAccount
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public TenantUser TenantUser { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public Account Account { get; set; }
    }
}