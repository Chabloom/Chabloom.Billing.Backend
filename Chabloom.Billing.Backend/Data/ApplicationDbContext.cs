// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using Chabloom.Billing.Backend.Models;
using Chabloom.Billing.Backend.Models.Auth;
using Chabloom.Billing.Backend.Models.MultiTenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chabloom.Billing.Backend.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }

        public DbSet<TenantAddress> TenantAddresses { get; set; }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Bill> Bills { get; set; }

        public DbSet<BillSchedule> BillSchedules { get; set; }

        public DbSet<UserAccount> UserAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Auth tables

            modelBuilder.Entity<User>()
                .ToTable("Users");
            modelBuilder.Entity<Role>()
                .ToTable("Roles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>()
                .ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>()
                .ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserRole<Guid>>()
                .ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserToken<Guid>>()
                .ToTable("UserTokens");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>()
                .ToTable("RoleClaims");

            // Use complex key based on name and tenant id
            modelBuilder.Entity<User>()
                .HasAlternateKey(x => new {x.UserName, x.TenantId});
            // Use complex key based on name and tenant id
            modelBuilder.Entity<Role>()
                .HasAlternateKey(x => new {x.Name, x.TenantId});

            #endregion

            #region Auth data

            modelBuilder.Entity<Tenant>()
                .HasData(Demo1Data.Tenant);
            modelBuilder.Entity<Tenant>()
                .HasData(Demo2Data.Tenant);
            modelBuilder.Entity<Role>()
                .HasData(Demo1Data.TenantRoles);
            modelBuilder.Entity<Role>()
                .HasData(Demo2Data.TenantRoles);

            #endregion

            #region Application tables

            // Set up key for join table
            modelBuilder.Entity<UserAccount>()
                .HasKey(x => new {x.UserId, x.AccountId});

            #endregion

            #region Application data

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

            #endregion
        }
    }
}