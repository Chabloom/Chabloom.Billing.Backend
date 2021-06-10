// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Chabloom.Billing.Backend.Models.Accounts;

namespace Chabloom.Billing.Backend.Models.Bills
{
    public class Bill
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

        public string PaymentId { get; set; }

        public string PaymentScheduleId { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime DueDate { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public Account Account { get; set; }

        #region Auditing

        [Required]
        public Guid CreatedUser { get; set; }

        [Required]
        public DateTimeOffset CreatedTimestamp { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        public Guid? UpdatedUser { get; set; }

        [Required]
        public DateTimeOffset? UpdatedTimestamp { get; set; }

        [Required]
        public Guid? DisabledUser { get; set; }

        [Required]
        public DateTimeOffset? DisabledTimestamp { get; set; }

        #endregion
    }
}