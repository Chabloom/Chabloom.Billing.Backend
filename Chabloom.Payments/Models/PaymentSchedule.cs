// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chabloom.Payments.Models
{
    public class PaymentSchedule
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(255)]
        public string Currency { get; set; }

        [Required]
        public int Day { get; set; }

        [Required]
        public int MonthInterval { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime BeginDate { get; set; } = DateTime.MinValue;

        [Required]
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; } = DateTime.MaxValue;

        [Required]
        public Account Account { get; set; }

        public IList<Payment> Payments { get; set; }

        #region Transaction

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