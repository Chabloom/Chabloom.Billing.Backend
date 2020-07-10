// Copyright 2020 Chabloom LC. All rights reserved.

using System;
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

        public DbSet<Bill> Bills { get; set; }

        public DbSet<Partition> Partitions { get; set; }

        public DbSet<PaymentSchedule> PaymentSchedules { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder?.Entity<Partition>().HasData(
                new Partition
                {
                    Id = Guid.Parse("040B9DA1-352D-4B81-8E6A-78ECD2FF14D1"),
                    Name = "Default"
                });
        }
    }
}