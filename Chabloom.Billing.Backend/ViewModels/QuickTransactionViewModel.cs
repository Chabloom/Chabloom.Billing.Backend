// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Chabloom.Billing.Backend.ViewModels
{
    public class QuickTransactionViewModel
    {
        [Required]
        public Guid BillId { get; set; }

        [Required]
        public string TransactionId { get; set; }
    }
}