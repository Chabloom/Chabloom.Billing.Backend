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

        public DbSet<AccountRole> AccountRoles { get; set; }

        public DbSet<AccountUser> AccountUsers { get; set; }

        public DbSet<AccountUserRole> AccountUserRoles { get; set; }

        public DbSet<Bill> Bills { get; set; }

        public DbSet<BillSchedule> BillSchedules { get; set; }

        public DbSet<BillTransaction> BillTransactions { get; set; }

        public DbSet<Tenant> Tenants { get; set; }

        public DbSet<TenantRole> TenantRoles { get; set; }

        public DbSet<TenantUser> TenantUsers { get; set; }

        public DbSet<TenantUserRole> TenantUserRoles { get; set; }
    }
}