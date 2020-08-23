// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Payments.ViewModels
{
    public class BillScheduleViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public int DayDue { get; set; }

        [Required]
        public int Interval { get; set; }

        [Required]
        public Guid Account { get; set; }
    }
}