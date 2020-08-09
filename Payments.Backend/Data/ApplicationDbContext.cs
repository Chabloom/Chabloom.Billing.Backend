// Copyright 2020 Chabloom LC. All rights reserved.

using Microsoft.EntityFrameworkCore;
using Payments.Backend.Models;

namespace Payments.Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Bill> Bills { get; set; }

        public DbSet<BillSchedule> BillSchedules { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
    }
}