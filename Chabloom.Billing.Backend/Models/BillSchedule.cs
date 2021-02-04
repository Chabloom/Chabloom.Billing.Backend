// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chabloom.Billing.Backend.Models
{
    public class BillSchedule
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
        public string Currency { get; set; } = "USD";

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

        public string TransactionScheduleId { get; set; }

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
        public Guid UpdatedUser { get; set; } = Guid.Empty;

        [Required]
        public DateTimeOffset UpdatedTimestamp { get; set; } = DateTimeOffset.MinValue;

        [Required]
        public bool Disabled { get; set; } = false;

        [Required]
        public Guid DisabledUser { get; set; } = Guid.Empty;

        [Required]
        public DateTimeOffset DisabledTimestamp { get; set; } = DateTimeOffset.MinValue;

        #endregion
    }
}