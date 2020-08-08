// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Payments.Backend.Data;

namespace Payments.Backend.Models
{
    public class Account
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public string ExternalId { get; set; }

        #region Auditing

        public string CreatedUser { get; set; }

        [Required]
        public DateTimeOffset CreatedTimestamp { get; set; } = DateTimeOffset.UtcNow;

        public string UpdatedUser { get; set; }

        [Required]
        public DateTimeOffset UpdatedTimestamp { get; set; } = DateTimeOffset.UtcNow;

        #endregion

        #region Foreign Keys

        [Required]
        public ApplicationUser Owner { get; set; }

        public List<Bill> Bills { get; set; }

        public List<BillSchedule> BillSchedules { get; set; }

        #endregion
    }
}