// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Payments.Backend.Models
{
    public class AccountViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string PrimaryAddress { get; set; }

        [Required]
        public bool Enabled { get; set; } = true;

        public List<Guid> PaymentSchedules { get; set; }

        public List<Guid> Bills { get; set; }

        public Guid Partition { get; set; }
    }
}