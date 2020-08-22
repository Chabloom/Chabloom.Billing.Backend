// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chabloom.Payments.Models
{
    [Table("PaymentsAccountRoles")]
    public class AccountRole
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public Account Account { get; set; }

        #region Auditing

        [Required]
        public string CreatedUser { get; set; }

        [Required]
        public DateTimeOffset CreatedTimestamp { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        public string UpdatedUser { get; set; }

        [Required]
        public DateTimeOffset UpdatedTimestamp { get; set; } = DateTimeOffset.UtcNow;

        #endregion
    }
}