// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Billing.Backend.Models.MultiTenant
{
    public class TenantAddress
    {
        [Required]
        [Key]
        public string Address { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Tenant Tenant { get; set; }
    }
}