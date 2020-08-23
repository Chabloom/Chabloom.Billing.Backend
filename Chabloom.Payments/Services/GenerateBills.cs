// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using Chabloom.Payments.Data;
using Chabloom.Payments.Models;
using Microsoft.EntityFrameworkCore;

namespace Chabloom.Payments.Services
{
    public class GenerateBills
    {
        private readonly ApplicationDbContext _context;

        public GenerateBills(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Run()
        {
            // Get enabled bill schedules that do not have a bill in the future
            var schedules = await _context.Schedules
                .Include(x => x.Account)
                .ThenInclude(x => x.Bills)
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);

            // TODO: Account for end of year and end of month
            var newBills = (from schedule in schedules
                let dueDates = schedule.Account.Bills
                    .Where(x => x.DueDate >= DateTime.UtcNow.Date)
                where !dueDates.Any()
                select new Bill
                {
                    Name = $"{schedule.Name}",
                    Amount = schedule.Amount,
                    DueDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, schedule.DayDue),
                    Account = schedule.Account,
                    CreatedUser = Guid.Empty,
                    UpdatedUser = Guid.Empty,
                    DisabledUser = Guid.Empty
                }).ToList();

            // Add the bills to the database
            await _context.AddRangeAsync(newBills)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // TODO: Send notifications to users
        }
    }
}