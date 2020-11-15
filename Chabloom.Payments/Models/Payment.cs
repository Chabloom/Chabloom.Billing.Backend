// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chabloom.Payments.Models
{
    public class Payment
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(255)]
        public string Currency { get; set; } = "USD";

        [Required]
        [Column(TypeName = "date")]
        public DateTime DueDate { get; set; }

        [Required]
        public bool Complete { get; set; } = false;

        [Required]
        public Account Account { get; set; }

        public PaymentSchedule PaymentSchedule { get; set; }

        #region Transaction

        public Guid TransactionId { get; set; }

        public Guid TransactionScheduleId { get; set; }

        #endregion

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