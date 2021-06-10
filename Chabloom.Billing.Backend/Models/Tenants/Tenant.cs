// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Chabloom.Billing.Backend.Models.Users;

namespace Chabloom.Billing.Backend.Models.Tenants
{
    public class Tenant
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        public List<TenantHost> Addresses { get; set; }

        public List<TenantRole> Roles { get; set; }

        public List<TenantUser> Users { get; set; }
    }
}