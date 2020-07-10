// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Backend.Models
{
    public class TransactionViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string ExternalId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTimeOffset Timestamp { get; set; }

        [Required]
        public Guid Bill { get; set; }
    }
}