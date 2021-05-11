// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using Chabloom.Billing.Backend.Models;

// ReSharper disable StringLiteralTypo

namespace Chabloom.Billing.Backend.Data
{
    public static class Demo2Data
    {
        public static Tenant Tenant { get; } = new()
        {
            Id = Guid.Parse("7BA3A979-5ABF-407F-AEE1-75E2D5522711"),
            Name = "Aiken",
            CreatedUser = Guid.Empty,
            CreatedTimestamp = DateTimeOffset.MinValue
        };

        public static List<Account> Accounts { get; } = new()
        {
            new Account
            {
                Id = Guid.Parse("31A59E75-197F-433C-B6B4-6160C6CACE42"),
                Name = "400 Richland Ave",
                Address = "400 Richland Ave",
                ReferenceId = "12345",
                TenantId = Tenant.Id,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Account
            {
                Id = Guid.Parse("D1EAFFB6-5757-4FCA-B15F-04EC11073275"),
                Name = "402 Richland Ave",
                Address = "402 Richland Ave",
                ReferenceId = "12346",
                TenantId = Tenant.Id,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Account
            {
                Id = Guid.Parse("8A276669-64F9-4057-A741-10B6B5FF92B0"),
                Name = "403 Richland Ave",
                Address = "403 Richland Ave",
                ReferenceId = "12347",
                TenantId = Tenant.Id,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Account
            {
                Id = Guid.Parse("E89EE340-96D2-49B2-A7C9-F444FB95C148"),
                Name = "404 Richland Ave",
                Address = "404 Richland Ave",
                ReferenceId = "12348",
                TenantId = Tenant.Id,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            }
        };

        public static List<Bill> Bills { get; } = new()
        {
            new Bill
            {
                Id = Guid.Parse("EB54F672-A3AA-458C-8B4D-18CACAE438D6"),
                Name = "August 2021",
                Amount = 33.71M,
                AccountId = Accounts[0].Id,
                DueDate = new DateTime(2021, 9, 1),
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Bill
            {
                Id = Guid.Parse("B0EB88DC-C8EA-45C2-80A7-8ABF5978CEE5"),
                Name = "August 2021",
                Amount = 42.52M,
                AccountId = Accounts[1].Id,
                DueDate = new DateTime(2021, 9, 1),
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Bill
            {
                Id = Guid.Parse("61CE9459-55AA-452E-BD29-5E31913E42FF"),
                Name = "August 2021",
                Amount = 76.22M,
                AccountId = Accounts[2].Id,
                DueDate = new DateTime(2021, 9, 1),
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Bill
            {
                Id = Guid.Parse("27BC4CDC-E4F8-43DF-A23D-4136D371254B"),
                Name = "August 2021",
                Amount = 36.88M,
                AccountId = Accounts[3].Id,
                DueDate = new DateTime(2021, 9, 1),
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            }
        };

        public static List<BillSchedule> BillSchedules { get; } = new()
        {
            new BillSchedule
            {
                Id = Guid.Parse("600D7C6F-6B6A-4A0C-A62E-C5ED482EBC09"),
                Name = "Monthly Water",
                Amount = 33.50M,
                AccountId = Accounts[0].Id,
                Day = 1,
                MonthInterval = 1,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new BillSchedule
            {
                Id = Guid.Parse("35A5CC23-E120-48FE-AFDA-D42C26233193"),
                Name = "Monthly Water",
                Amount = 33.50M,
                AccountId = Accounts[1].Id,
                Day = 1,
                MonthInterval = 1,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new BillSchedule
            {
                Id = Guid.Parse("BFB53864-8389-4038-9B49-60BBB1358158"),
                Name = "Monthly Water",
                Amount = 33.50M,
                AccountId = Accounts[2].Id,
                Day = 1,
                MonthInterval = 1,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new BillSchedule
            {
                Id = Guid.Parse("CF28D12E-E51A-491C-ADA8-CD6FCF840E88"),
                Name = "Monthly Water",
                Amount = 33.50M,
                AccountId = Accounts[3].Id,
                Day = 1,
                MonthInterval = 1,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            }
        };
    }
}