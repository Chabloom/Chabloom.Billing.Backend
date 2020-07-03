// Copyright 2020 Chabloom LC. All rights reserved.

using Microsoft.EntityFrameworkCore;
using Payments.Backend.Models;

namespace Payments.Backend.Data
{
    public class PaymentServiceDbContext : DbContext
    {
        public PaymentServiceDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Address> Addresses { get; set; }
    }
}