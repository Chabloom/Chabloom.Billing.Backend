// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Backend.Data;
using Payments.Backend.Models;
using Stripe;

namespace Payments.Backend.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly PaymentServiceDbContext _context;

        public TransactionsController(PaymentServiceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<TransactionViewModel>>> GetTransactions()
        {
            return await _context.Transactions
                .Include(x => x.Bill)
                .Select(x => new TransactionViewModel
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    ReferenceId = x.ReferenceId,
                    Timestamp = x.Timestamp,
                    Bill = x.Bill.Id
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TransactionViewModel>> GetTransaction(Guid id)
        {
            var transaction = await _context.Transactions
                .Include(x => x.Bill)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (transaction == null)
            {
                return NotFound();
            }

            return new TransactionViewModel
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                ReferenceId = transaction.ReferenceId,
                Timestamp = transaction.Timestamp,
                Bill = transaction.Bill.Id
            };
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TransactionViewModel>> PostTransaction(TransactionViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null)
            {
                return BadRequest();
            }

            var transaction = new Transaction
            {
                Amount = viewModel.Amount,
                ReferenceId = viewModel.ReferenceId,
                Timestamp = DateTimeOffset.UtcNow,
                Bill = await _context.Bills
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Bill)
                    .ConfigureAwait(false)
            };

            await _context.Transactions.AddAsync(transaction)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = transaction.Id;

            return CreatedAtAction("GetTransaction", new {id = viewModel.Id}, viewModel);
        }

        [HttpPost("Intent")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<string>> PostTransactionIntent(TransactionViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null)
            {
                return BadRequest();
            }

            StripeConfiguration.ApiKey = "sk_test_51H6f58B2z6MUftpdlRIg5n2dTmaNRD2gLfGNQEHyYWHSyC5DUa6hVsZKBLj9w1Lg5LwSVnueZxD6xfsjlMera5L100BDz1OkHn";

            var options = new PaymentIntentCreateOptions
            {
                Amount = viewModel.Amount,
                Currency = "usd",
                Metadata = new Dictionary<string, string>
                {
                    { "integration_check", "accept_a_payment" },
                },
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options)
                .ConfigureAwait(false);

            return Ok(paymentIntent.ClientSecret);
        }
    }
}