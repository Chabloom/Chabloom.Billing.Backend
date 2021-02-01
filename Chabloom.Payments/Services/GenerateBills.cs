// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System.Threading.Tasks;
using Chabloom.Payments.Data;

namespace Chabloom.Payments.Services
{
    public class GenerateBills
    {
        private readonly ApplicationDbContext _context;

        public GenerateBills(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task Run()
        {
            return Task.CompletedTask;
        }
    }
}