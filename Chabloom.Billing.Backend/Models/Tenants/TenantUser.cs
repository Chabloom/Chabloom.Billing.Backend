// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Chabloom.Billing.Backend.Models.Tenants
{
    public class TenantUser : IdentityUser<Guid>
    {
        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Tenant Tenant { get; set; }
    }
}