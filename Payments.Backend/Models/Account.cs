// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Payments.Backend.Models
{
    [Table("PaymentsAccounts")]
    public class Account
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public string PrimaryAddress { get; set; }

        [Required]
        public bool Enabled { get; set; } = true;

        public List<PaymentSchedule> PaymentSchedules { get; set; }

        public List<Bill> Bills { get; set; }

        [Required]
        public Partition Partition { get; set; }
    }
}