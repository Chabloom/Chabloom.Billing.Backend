// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chabloom.Payments.Models
{
    [Table("PaymentsApplicationRoles")]
    public class ApplicationRole
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public Account Account { get; set; }

        public List<ApplicationUser> Users { get; set; }

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