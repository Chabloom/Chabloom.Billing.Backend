// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Payments.Backend.Models
{
    public class Account
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        public List<Address> Addresses { get; set; }

        public AccountViewModel ToViewModel()
        {
            return new AccountViewModel
            {
                Id = Id,
                Name = Name
            };
        }
    }
}