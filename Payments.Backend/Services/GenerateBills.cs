// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Payments.Backend.Data;
using Payments.Backend.Models;

namespace Payments.Backend.Services
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
            // TODO: Allow delay after due date to be configured
            var billSchedules = await _context.BillSchedules
                .Include(x => x.Account)
                .Include(x => x.Bills)
                .Where(x => x.Enabled)
                .Where(x => x.Bills.Where(y => y.DueDate >= DateTime.UtcNow.Date.AddDays(10)).ToList().Count == 0)
                .ToListAsync()
                .ConfigureAwait(false);

            // Create bills for the bill schedules
            // TODO: Account for end of year and end of month
            var bills = billSchedules.Select(schedule => new Bill
            {
                Name = $"{schedule.Name}",
                Amount = schedule.Amount,
                DueDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, schedule.DayDue),
                Account = schedule.Account,
                BillSchedule = schedule,
                CreatedUser = "GenerateBills",
                UpdatedUser = "GenerateBills"
            });

            // Add the bills to the database
            await _context.AddRangeAsync(bills)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // TODO: Send notifications to users
        }
    }
}