// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using Chabloom.Billing.Backend.Models;
using Chabloom.Billing.Backend.Models.Auth;
using Chabloom.Billing.Backend.Models.MultiTenant;

// ReSharper disable StringLiteralTypo

namespace Chabloom.Billing.Backend.Data
{
    public static class Demo1Data
    {
        public static Tenant Tenant { get; } = new()
        {
            Id = Guid.Parse("A2CEE23F-3250-4B1B-93DC-87443B02DD89"),
            Name = "North Augusta"
        };

        public static List<Role> TenantRoles { get; } = new()
        {
            new Role
            {
                Id = Guid.Parse("99A3B0A6-0ADB-4FEE-B2CC-380EE21EA446"),
                Name = "Admin",
                ConcurrencyStamp = "c4558764-0f55-4538-ae3a-ad6c8f8124dc",
                TenantId = Tenant.Id
            },
            new Role
            {
                Id = Guid.Parse("F94C10F9-69DD-459F-A2FE-3BE09C2C4075"),
                Name = "Manager",
                ConcurrencyStamp = "127fb553-75cd-4c9a-932f-b5b036d40505",
                TenantId = Tenant.Id
            }
        };

        public static List<Account> Accounts { get; } = new()
        {
            new Account
            {
                Id = Guid.Parse("0C994055-DFE2-4863-A087-2B47CD2A1E25"),
                Name = "400 W Martintown Rd",
                Address = "400 W Martintown Rd",
                ReferenceId = "12345",
                TenantId = Tenant.Id,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Account
            {
                Id = Guid.Parse("C68DDF06-20F6-41F0-869A-6F39CD8D9431"),
                Name = "402 W Martintown Rd",
                Address = "402 W Martintown Rd",
                ReferenceId = "12346",
                TenantId = Tenant.Id,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Account
            {
                Id = Guid.Parse("5A79115E-2E5A-4FCA-9EA1-E6A21E7DE4D4"),
                Name = "403 W Martintown Rd",
                Address = "403 W Martintown Rd",
                ReferenceId = "12347",
                TenantId = Tenant.Id,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Account
            {
                Id = Guid.Parse("1BA99ECC-48D3-4839-B748-7565E2A72F77"),
                Name = "404 W Martintown Rd",
                Address = "404 W Martintown Rd",
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
                Id = Guid.Parse("7690274E-C4AD-4964-9CC5-3625CCAABCCA"),
                Name = "August 2021",
                Amount = 4329,
                CurrencyId = "USD",
                AccountId = Accounts[0].Id,
                DueDate = new DateTime(2021, 9, 1),
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Bill
            {
                Id = Guid.Parse("8BE7E620-7F46-4460-A49A-DE09B8895640"),
                Name = "August 2021",
                Amount = 5521,
                CurrencyId = "USD",
                AccountId = Accounts[1].Id,
                DueDate = new DateTime(2021, 9, 1),
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Bill
            {
                Id = Guid.Parse("8240327D-547A-4875-92C8-B1DCFA5F13A9"),
                Name = "August 2021",
                Amount = 6123,
                CurrencyId = "USD",
                AccountId = Accounts[2].Id,
                DueDate = new DateTime(2021, 9, 1),
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Bill
            {
                Id = Guid.Parse("6EEC413F-F55E-4DAC-8A6D-2F38E5A04FE7"),
                Name = "August 2021",
                Amount = 4591,
                CurrencyId = "USD",
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
                Id = Guid.Parse("BE05AE00-081D-48D4-B047-E93FB7523934"),
                Name = "Monthly Water",
                Amount = 3350,
                CurrencyId = "USD",
                AccountId = Accounts[0].Id,
                Day = 1,
                MonthInterval = 1,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new BillSchedule
            {
                Id = Guid.Parse("298AA43A-6376-4049-AE6E-6C0D1741FCC4"),
                Name = "Monthly Water",
                Amount = 3350,
                CurrencyId = "USD",
                AccountId = Accounts[1].Id,
                Day = 1,
                MonthInterval = 1,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new BillSchedule
            {
                Id = Guid.Parse("D28F8EC7-E716-4D5A-B0C4-832D9A1134D5"),
                Name = "Monthly Water",
                Amount = 3350,
                CurrencyId = "USD",
                AccountId = Accounts[2].Id,
                Day = 1,
                MonthInterval = 1,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new BillSchedule
            {
                Id = Guid.Parse("38FBB0FE-B1B3-42A9-B591-88C37381E5BC"),
                Name = "Monthly Water",
                Amount = 3350,
                CurrencyId = "USD",
                AccountId = Accounts[3].Id,
                Day = 1,
                MonthInterval = 1,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            }
        };
    }
}