// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Payments.Backend.Data;

namespace Payments.Backend.Models
{
    public class BillSchedule
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
        public int DayDue { get; set; }

        [Required]
        public int MonthInterval { get; set; }

        [Required]
        public bool Enabled { get; set; } = true;

        #region Authorization

        [Required]
        public Account Account { get; set; }

        #endregion

        #region Auditing

        public ApplicationUser CreatedAccount { get; set; }

        [Required]
        public DateTimeOffset CreatedTimestamp { get; set; } = DateTimeOffset.UtcNow;

        public ApplicationUser UpdatedAccount { get; set; }

        [Required]
        public DateTimeOffset UpdatedTimestamp { get; set; } = DateTimeOffset.UtcNow;

        #endregion

        #region Foreign Keys

        public List<Bill> Bills { get; set; }

        #endregion
    }
}