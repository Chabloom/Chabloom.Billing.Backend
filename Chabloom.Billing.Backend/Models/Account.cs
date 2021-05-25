// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Chabloom.Billing.Backend.Models.Auth;

namespace Chabloom.Billing.Backend.Models
{
    public class Account
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string ReferenceId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Tenant Tenant { get; set; }

        public List<Bill> Bills { get; set; }

        public List<BillSchedule> BillSchedules { get; set; }

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