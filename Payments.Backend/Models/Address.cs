// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Backend.Models
{
    public class Address
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public Account Account { get; set; }

        public AddressViewModel ToViewModel()
        {
            return new AddressViewModel
            {
                Id = Id,
                Name = Name
            };
        }
    }
}