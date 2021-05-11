// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;

namespace Chabloom.Billing.Backend.Services
{
    public class GenerateBills
    {
        private readonly BillingDbContext _context;

        public GenerateBills(BillingDbContext context)
        {
            _context = context;
        }

        public Task Run()
        {
            return Task.CompletedTask;
        }
    }
}