// Copyright 2020-2021 Chabloom LC. All rights reserved.

using Chabloom.Billing.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Chabloom.Billing.Backend.Data
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

        public DbSet<Bill> Bills { get; set; }

        public DbSet<BillSchedule> BillSchedules { get; set; }

        public DbSet<Tenant> Tenants { get; set; }

        public DbSet<TenantUser> TenantUsers { get; set; }
    }
}