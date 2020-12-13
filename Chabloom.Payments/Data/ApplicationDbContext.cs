// Copyright 2020 Chabloom LC. All rights reserved.

using Chabloom.Payments.Models;
using Microsoft.EntityFrameworkCore;

namespace Chabloom.Payments.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<AccountUser> AccountUsers { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<Tenant> Tenants { get; set; }

        public DbSet<TenantUser> TenantUsers { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<PaymentSchedule> PaymentSchedules { get; set; }
    }
}