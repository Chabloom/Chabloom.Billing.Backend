// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Billing.Backend.ViewModels
{
    public class ApplicationUserViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }
}