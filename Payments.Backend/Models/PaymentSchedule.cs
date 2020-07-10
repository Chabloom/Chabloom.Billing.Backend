// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
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
        [Column(TypeName = "decimal(18,4)")]
        public decimal Amount { get; set; }

        [Required]
        public int DayDue { get; set; }

        [Required]
        public int Interval { get; set; }

        [Required]
        public List<Account> Accounts { get; set; }
    }
}