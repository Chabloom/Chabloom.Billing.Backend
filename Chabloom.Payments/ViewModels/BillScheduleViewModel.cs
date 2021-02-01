// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Payments.ViewModels
{
    public class BillScheduleViewModel
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