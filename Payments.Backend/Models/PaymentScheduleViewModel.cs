// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Backend.Models
{
    public class PaymentScheduleViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Amount { get; set; }

        [Required]
        public int DayDue { get; set; }

        [Required]
        public int MonthInterval { get; set; }

        [Required]
        public bool Enabled { get; set; } = true;

        [Required]
        public Guid Account { get; set; }
    }
}