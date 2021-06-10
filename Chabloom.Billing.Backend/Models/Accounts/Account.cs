// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Chabloom.Billing.Backend.Models.Bills;
using Chabloom.Billing.Backend.Models.Tenants;

namespace Chabloom.Billing.Backend.Models.Accounts
{
    public class Account
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string TenantLookupId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

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

        public Guid? UpdatedUser { get; set; }

        public DateTimeOffset? UpdatedTimestamp { get; set; }

        public Guid? DisabledUser { get; set; }

        public DateTimeOffset? DisabledTimestamp { get; set; }

        #endregion
    }
}