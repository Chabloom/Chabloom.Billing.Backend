// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using Chabloom.Billing.Backend.Models.Accounts;
using Chabloom.Billing.Backend.Models.Bills;
using Chabloom.Billing.Backend.Models.Tenants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chabloom.Billing.Backend.Data
{
    public class ApplicationDbContext : IdentityDbContext<TenantUser, TenantRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }

        public DbSet<TenantHost> TenantHosts { get; set; }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<UserAccount> UserAccounts { get; set; }

        public DbSet<Bill> Bills { get; set; }

        public DbSet<BillSchedule> BillSchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Auth tables

            modelBuilder.Entity<TenantUser>()
                .ToTable("TenantUsers");
            modelBuilder.Entity<TenantRole>()
                .ToTable("TenantRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>()
                .ToTable("TenantUserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>()
                .ToTable("TenantUserLogins");
            modelBuilder.Entity<IdentityUserRole<Guid>>()
                .ToTable("TenantUserRoles");
            modelBuilder.Entity<IdentityUserToken<Guid>>()
                .ToTable("TenantUserTokens");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>()
                .ToTable("TenantRoleClaims");

            // Use complex key based on name and tenant id
            modelBuilder.Entity<TenantUser>()
                .HasAlternateKey(x => new {x.UserName, x.TenantId});
            // Use complex key based on name and tenant id
            modelBuilder.Entity<TenantRole>()
                .HasAlternateKey(x => new {x.Name, x.TenantId});

            #endregion

            #region Auth data

            modelBuilder.Entity<Tenant>()
                .HasData(Demo1Data.Tenant);
            modelBuilder.Entity<Tenant>()
                .HasData(Demo2Data.Tenant);
            modelBuilder.Entity<TenantRole>()
                .HasData(Demo1Data.TenantRoles);
            modelBuilder.Entity<TenantRole>()
                .HasData(Demo2Data.TenantRoles);

            #endregion

            #region Application tables

            // Set up alternate unique key for lookup id
            modelBuilder.Entity<Account>()
                .HasAlternateKey(x => new {x.TenantId, x.TenantLookupId});

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