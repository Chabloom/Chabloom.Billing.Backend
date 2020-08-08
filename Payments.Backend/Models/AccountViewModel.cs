// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Backend.Models
{
    public class AccountViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string PrimaryAddress { get; set; }

        [Required]
        public string ExternalId { get; set; }

        public string OwnerId { get; set; }
    }
}