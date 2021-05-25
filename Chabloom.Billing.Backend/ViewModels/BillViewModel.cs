// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Billing.Backend.ViewModels
{
    public class BillViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public ulong Amount { get; set; }

        [Required]
        public string CurrencyId { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public string TransactionId { get; set; }

        [Required]
        public Guid AccountId { get; set; }
    }
}