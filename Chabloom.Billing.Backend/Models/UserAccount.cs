// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using Chabloom.Billing.Backend.Models.Auth;

namespace Chabloom.Billing.Backend.Models
{
    public class UserAccount
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public User User { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public Account Account { get; set; }
    }
}