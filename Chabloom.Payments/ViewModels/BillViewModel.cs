// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Payments.ViewModels
{
    public class BillViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(255)]
        public string Currency { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public string TransactionId { get; set; }

        [Required]
        public Guid AccountId { get; set; }
    }
}