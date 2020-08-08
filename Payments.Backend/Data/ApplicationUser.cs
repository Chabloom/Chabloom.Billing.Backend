// Copyright 2020 Chabloom LC. All rights reserved.

using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Payments.Backend.Models;

namespace Payments.Backend.Data
{
    public class ApplicationUser : IdentityUser
    {
        public List<Account> Accounts { get; set; }
    }
}