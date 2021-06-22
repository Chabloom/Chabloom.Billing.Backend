// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using Chabloom.Billing.Backend.Models.Accounts;
using Chabloom.Billing.Backend.Models.Bills;
using Chabloom.Billing.Backend.Models.Tenants;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chabloom.Billing.Backend.Data
{
    public class ApplicationDbContext : IdentityDbContext<TenantUser, TenantRole, Guid>, IDataProtectionKeyContext
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

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Auth tables

            modelBuilder.Entity<TenantUser>(builder =>
            {
                // Make predefined index non-unique
                builder.HasIndex(x => x.NormalizedUserName)
                    .HasDatabaseName("UserNameIndex")
                    .IsUnique(false);
                // Add tenant-based key
                builder.HasAlternateKey(x => new {x.NormalizedUserName, x.TenantId});
                builder.ToTable("TenantUsers");
            });
            modelBuilder.Entity<TenantRole>(builder =>
            {
                // Make predefined index non-unique
                builder.HasIndex(x => x.NormalizedName)
                    .HasDatabaseName("RoleNameIndex")
                    .IsUnique(false);
                // Add tenant-based key
                builder.HasAlternateKey(x => new {x.NormalizedName, x.TenantId});
                builder.ToTable("TenantRoles");
            });
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

            #endregion

            #region Auth data

            modelBuilder.Entity<Tenant>(builder =>
            {
                builder.HasData(Demo1Data.Tenant);
                builder.HasData(Demo2Data.Tenant);
            });
            modelBuilder.Entity<TenantHost>(builder =>
            {
                builder.HasData(Demo1Data.TenantHosts);
                builder.HasData(Demo2Data.TenantHosts);
            });
            modelBuilder.Entity<TenantRole>(builder =>
            {
                builder.HasData(Demo1Data.TenantRoles);
                builder.HasData(Demo2Data.TenantRoles);
            });
            modelBuilder.Entity<TenantUser>(builder =>
            {
                builder.HasData(Demo1Data.TenantUsers);
                builder.HasData(Demo2Data.TenantUsers);
            });
            modelBuilder.Entity<IdentityUserClaim<Guid>>(builder =>
            {
                builder.HasData(Demo1Data.TenantUserClaims);
                builder.HasData(Demo2Data.TenantUserClaims);
            });
            modelBuilder.Entity<IdentityUserRole<Guid>>(builder =>
            {
                builder.HasData(Demo1Data.TenantUserRoles);
                builder.HasData(Demo2Data.TenantUserRoles);
            });

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