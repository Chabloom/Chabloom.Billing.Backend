// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Payments.ViewModels.PaymentSchedule
{
    public class InitViewModel
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [MaxLength(255)]
        public string Currency { get; set; }

        [Required]
        public int Day { get; set; }

        [Required]
        public int MonthInterval { get; set; }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        public Guid Account { get; set; }
    }
}