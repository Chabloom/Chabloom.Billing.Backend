// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using Chabloom.Billing.Backend.Models.Accounts;

namespace Chabloom.Billing.Backend.Models.Bills
{
    public class BillSchedule
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public ulong Amount { get; set; }

        [Required]
        public string CurrencyId { get; set; }

        public string PaymentScheduleId { get; set; }

        [Required]
        public int Day { get; set; }

        [Required]
        public int MonthInterval { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public Account Account { get; set; }
    }
}