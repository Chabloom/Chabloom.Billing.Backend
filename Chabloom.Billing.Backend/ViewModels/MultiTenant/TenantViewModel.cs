// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Billing.Backend.ViewModels.MultiTenant
{
    public class TenantViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}