// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Payments.Models
{
    public class TransactionViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ExternalId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public Guid Account { get; set; }

        public Guid Bill { get; set; }
    }
}