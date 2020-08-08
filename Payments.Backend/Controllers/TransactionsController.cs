// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Backend.Data;
using Payments.Backend.Models;

namespace Payments.Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
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
                    Name = x.Name,
                    ExternalId = x.ExternalId,
                    Amount = x.Amount,
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
                Name = transaction.Name,
                ExternalId = transaction.ExternalId,
                Amount = transaction.Amount,
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
                Name = viewModel.Name,
                ExternalId = viewModel.ExternalId,
                Amount = viewModel.Amount,
                Bill = await _context.Bills
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Bill)
                    .ConfigureAwait(false),
                CreatedUser = User.GetDisplayName()
            };

            await _context.Transactions.AddAsync(transaction)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = transaction.Id;

            return CreatedAtAction("GetTransaction", new {id = viewModel.Id}, viewModel);
        }
    }
}