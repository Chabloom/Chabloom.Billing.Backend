// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Billing.Backend.ViewModels
{
    public class BillScheduleViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public ulong Amount { get; set; }

        [Required]
        public string CurrencyId { get; set; }

        [Required]
        public int Day { get; set; }

        [Required]
        public int MonthInterval { get; set; }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public string TransactionScheduleId { get; set; }

        [Required]
        public Guid AccountId { get; set; }
    }
}