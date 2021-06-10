// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Billing.Backend.ViewModels.Accounts
{
    public class UserAccountViewModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid AccountId { get; set; }
    }
}