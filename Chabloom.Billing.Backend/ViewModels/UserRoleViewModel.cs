// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Billing.Backend.ViewModels
{
    public class UserRoleViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public Guid TenantId { get; set; }
    }
}