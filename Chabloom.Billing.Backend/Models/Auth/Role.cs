// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using Chabloom.Billing.Backend.Models.MultiTenant;
using Microsoft.AspNetCore.Identity;

namespace Chabloom.Billing.Backend.Models.Auth
{
    public class Role : IdentityRole<Guid>
    {
        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Tenant Tenant { get; set; }
    }
}