// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;

namespace Payments.Backend.Models
{
    public class AddressViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public Address ToModel()
        {
            return new Address
            {
                Id = Id,
                Name = Name
            };
        }
    }
}