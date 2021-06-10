// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Billing.Backend.ViewModels.Tenants
{
    public class TenantHostViewModel
    {
        [Required]
        public string Hostname { get; set; }

        [Required]
        public Guid TenantId { get; set; }
    }
}