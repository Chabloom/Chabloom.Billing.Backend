// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Payments.ViewModels
{
    public class TenantUserRoleViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public Guid User { get; set; }

        [Required]
        public Guid Role { get; set; }
    }
}