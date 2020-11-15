// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Payments.ViewModels.Payment
{
    public class UpdateViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [MaxLength(255)]
        public string Currency { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public bool Complete { get; set; }

        public Guid Transaction { get; set; }

        public Guid TransactionSchedule { get; set; }
    }
}