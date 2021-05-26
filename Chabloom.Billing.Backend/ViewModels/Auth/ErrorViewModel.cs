// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Chabloom.Billing.Backend.ViewModels.Auth
{
    public class ErrorViewModel
    {
        [Required]
        public string Error { get; set; }

        [Required]
        public string ErrorDescription { get; set; }

        [Required]
        public string RedirectUri { get; set; }
    }
}