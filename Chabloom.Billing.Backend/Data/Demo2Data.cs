// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using Chabloom.Billing.Backend.Models;
using Chabloom.Billing.Backend.Models.Accounts;
using Chabloom.Billing.Backend.Models.Bills;
using Chabloom.Billing.Backend.Models.Tenants;
using IdentityModel;
using Microsoft.AspNetCore.Identity;

// ReSharper disable StringLiteralTypo

namespace Chabloom.Billing.Backend.Data
{
    public static class Demo2Data
    {
        public static Tenant Tenant { get; } = new()
        {
            Id = Guid.Parse("7BA3A979-5ABF-407F-AEE1-75E2D5522711"),
            Name = "Aiken"
        };

        public static List<TenantRole> TenantRoles { get; } = new()
        {
            new TenantRole
            {
                Id = Guid.Parse("191E5E91-0E14-460E-A481-2F00C72B8228"),
                Name = "Admin",
                ConcurrencyStamp = "e9da379a-ccf0-4209-9460-555d013831b1",
                TenantId = Tenant.Id
            },
            new TenantRole
            {
                Id = Guid.Parse("52C71AE1-9B6B-4694-9EA8-E70501A8ACA2"),
                Name = "Manager",
                ConcurrencyStamp = "6bad7203-2f73-4ba7-92b8-98ee8ad95f3f",
                TenantId = Tenant.Id
            }
        };

        public static List<TenantUser> TenantUsers { get; } = new()
        {
            new TenantUser
            {
                Id = Guid.Parse("DFB1359F-A11D-4DE1-B8AC-ADA45AEF0B72"),
                UserName = "mdcasey@chabloom.com",
                NormalizedUserName = "MDCASEY@CHABLOOM.COM",
                Email = "mdcasey@chabloom.com",
                NormalizedEmail = "MDCASEY@CHABLOOM.COM",
                EmailConfirmed = true,
                PhoneNumber = "+18036179564",
                PhoneNumberConfirmed = true,
                PasswordHash =
                    "AQAAAAEAACcQAAAAELYyWQtU3cVbIfdmk4LHrtYsKTiYVW7OAge27lolZ3I8D97OE4QQ6Yn4XwGhO8YPuQ==",
                SecurityStamp = "C3KZM3I2WQCCAD7EVHRZQSGRFRX5MY3I",
                ConcurrencyStamp = "CB0FCA5D-D2F3-4D0B-8277-2091FED613B3",
                TenantId = Tenant.Id
            }
        };

        public static List<IdentityUserClaim<Guid>> TenantUserClaims { get; } = new()
        {
            new IdentityUserClaim<Guid>
            {
                Id = 2,
                UserId = TenantUsers[0].Id,
                ClaimType = JwtClaimTypes.Name,
                ClaimValue = "Matthew Casey"
            }
        };

        public static List<IdentityUserRole<Guid>> TenantUserRoles { get; } = new()
        {
            new IdentityUserRole<Guid>
            {
                UserId = TenantUsers[0].Id,
                RoleId = TenantRoles[0].Id
            },
        };

        public static List<Account> Accounts { get; } = new()
        {
            new Account
            {
                Id = Guid.Parse("31A59E75-197F-433C-B6B4-6160C6CACE42"),
                Name = "400 Richland Ave",
                Address = "400 Richland Ave",
                TenantLookupId = "12345",
                TenantId = Tenant.Id,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Account
            {
                Id = Guid.Parse("D1EAFFB6-5757-4FCA-B15F-04EC11073275"),
                Name = "402 Richland Ave",
                Address = "402 Richland Ave",
                TenantLookupId = "12346",
                TenantId = Tenant.Id,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Account
            {
                Id = Guid.Parse("8A276669-64F9-4057-A741-10B6B5FF92B0"),
                Name = "403 Richland Ave",
                Address = "403 Richland Ave",
                TenantLookupId = "12347",
                TenantId = Tenant.Id,
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Account
            {
                Id = Guid.Parse("E89EE340-96D2-49B2-A7C9-F444FB95C148"),
                Name = "404 Richland Ave",
                Address = "404 Richland Ave",
                TenantLookupId = "12348",
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
                Amount = 3371,
                CurrencyId = "USD",
                AccountId = Accounts[0].Id,
                DueDate = new DateTime(2021, 9, 1),
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Bill
            {
                Id = Guid.Parse("B0EB88DC-C8EA-45C2-80A7-8ABF5978CEE5"),
                Name = "August 2021",
                Amount = 4252,
                CurrencyId = "USD",
                AccountId = Accounts[1].Id,
                DueDate = new DateTime(2021, 9, 1),
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Bill
            {
                Id = Guid.Parse("61CE9459-55AA-452E-BD29-5E31913E42FF"),
                Name = "August 2021",
                Amount = 7622,
                CurrencyId = "USD",
                AccountId = Accounts[2].Id,
                DueDate = new DateTime(2021, 9, 1),
                CreatedUser = Guid.Empty,
                CreatedTimestamp = DateTimeOffset.MinValue
            },
            new Bill
            {
                Id = Guid.Parse("27BC4CDC-E4F8-43DF-A23D-4136D371254B"),
                Name = "August 2021",
                Amount = 3688,
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
                Id = Guid.Parse("600D7C6F-6B6A-4A0C-A62E-C5ED482EBC09"),
                Name = "Monthly Water",
                Amount = 3350,
                CurrencyId = "USD",
                AccountId = Accounts[0].Id,
                Day = 1,
                MonthInterval = 1
            },
            new BillSchedule
            {
                Id = Guid.Parse("35A5CC23-E120-48FE-AFDA-D42C26233193"),
                Name = "Monthly Water",
                Amount = 3350,
                CurrencyId = "USD",
                AccountId = Accounts[1].Id,
                Day = 1,
                MonthInterval = 1
            },
            new BillSchedule
            {
                Id = Guid.Parse("BFB53864-8389-4038-9B49-60BBB1358158"),
                Name = "Monthly Water",
                Amount = 3350,
                CurrencyId = "USD",
                AccountId = Accounts[2].Id,
                Day = 1,
                MonthInterval = 1
            },
            new BillSchedule
            {
                Id = Guid.Parse("CF28D12E-E51A-491C-ADA8-CD6FCF840E88"),
                Name = "Monthly Water",
                Amount = 3350,
                CurrencyId = "USD",
                AccountId = Accounts[3].Id,
                Day = 1,
                MonthInterval = 1
            }
        };
    }
}