// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Payments.ViewModels
{
    public class AccountUserViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid Account { get; set; }
    }
}