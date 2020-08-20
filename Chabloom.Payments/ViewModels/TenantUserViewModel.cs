// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Payments.ViewModels
{
    public class TenantUserViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid Tenant { get; set; }
    }
}