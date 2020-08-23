// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chabloom.Payments.Models
{
    [Table("PaymentsTransactions")]
    public class Transaction
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public string ExternalId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        public Bill Bill { get; set; }

        #region Auditing

        [Required]
        public Guid CreatedUser { get; set; }

        [Required]
        public DateTimeOffset CreatedTimestamp { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        public Guid UpdatedUser { get; set; }

        [Required]
        public DateTimeOffset UpdatedTimestamp { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        public bool Disabled { get; set; } = false;

        [Required]
        public Guid DisabledUser { get; set; }

        [Required]
        public DateTimeOffset DisabledTimestamp { get; set; }

        #endregion
    }
}