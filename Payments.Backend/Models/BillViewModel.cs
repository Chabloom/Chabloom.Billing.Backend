// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Backend.Models
{
    public class BillViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public bool Completed { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public Guid Account { get; set; }
    }
}