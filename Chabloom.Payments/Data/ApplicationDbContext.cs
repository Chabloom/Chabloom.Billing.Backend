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

        public DbSet<Bill> Bills { get; set; }

        public DbSet<BillSchedule> BillSchedules { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
    }
}