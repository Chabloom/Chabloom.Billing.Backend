// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Payments.Backend.Models
{
    [Table("PaymentsBills")]
    public class Bill
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public bool Completed { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public Account Account { get; set; }

        [Required]
        public List<Transaction> Transactions { get; set; }
    }
}