// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Payments.Backend.Models
{
    [Table("PaymentsPaymentSchedules")]
    public class PaymentSchedule
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

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
        public Account Account { get; set; }
    }
}