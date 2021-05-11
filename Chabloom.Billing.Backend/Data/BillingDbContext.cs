// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using Chabloom.Billing.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Chabloom.Billing.Backend.Data
{
    public class BillingDbContext : DbContext
    {
        public BillingDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<AccountUser> AccountUsers { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<Bill> Bills { get; set; }

        public DbSet<BillSchedule> BillSchedules { get; set; }

        public DbSet<Tenant> Tenants { get; set; }

        public DbSet<TenantUser> TenantUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var applicationUsers = new List<ApplicationUser>
            {
                new()
                {
                    Id = Guid.Parse("B749B662-3E2B-4199-A11F-85CB1B6AB3BE"),
                    UserId = Guid.Parse("421bde72-5f81-451b-83b6-08d8d3b98c06"),
                    CreatedUser = Guid.Empty,
                    CreatedTimestamp = DateTimeOffset.MinValue
                }
            };

            modelBuilder.Entity<ApplicationUser>()
                .HasData(applicationUsers);

            modelBuilder.Entity<Tenant>()
                .HasData(Demo1Data.Tenant);
            modelBuilder.Entity<Tenant>()
                .HasData(Demo2Data.Tenant);

            modelBuilder.Entity<Account>()
                .HasData(Demo1Data.Accounts);
            modelBuilder.Entity<Account>()
                .HasData(Demo2Data.Accounts);

            modelBuilder.Entity<Bill>()
                .HasData(Demo1Data.Bills);
            modelBuilder.Entity<Bill>()
                .HasData(Demo2Data.Bills);

            modelBuilder.Entity<BillSchedule>()
                .HasData(Demo1Data.BillSchedules);
            modelBuilder.Entity<BillSchedule>()
                .HasData(Demo2Data.BillSchedules);
        }
    }
}