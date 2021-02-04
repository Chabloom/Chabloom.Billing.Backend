// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Models;
using Chabloom.Billing.Backend.Services;
using Chabloom.Billing.Backend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Billing.Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BillsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BillsController> _logger;
        private readonly IValidator _validator;

        public BillsController(ApplicationDbContext context, ILogger<BillsController> logger,
            IValidator validator)
        {
            _context = context;
            _logger = logger;
            _validator = validator;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<BillViewModel>>> GetBills(Guid accountId)
        {
            // Get all bills
            var bills = await _context.Bills
                .Where(x => x.AccountId == accountId)
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .ToListAsync()
                .ConfigureAwait(false);
            if (bills == null || !bills.Any())
            {
                return new List<BillViewModel>();
            }

            // Convert to view models
            var viewModels = bills
                .Select(x => new BillViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    DueDate = x.DueDate,
                    TransactionId = x.TransactionId,
                    AccountId = x.AccountId
                })
                .Distinct()
                .ToList();

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<Bill>> GetBill(Guid id)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified bill
            var viewModel = await _context.Bills
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .Select(x => new BillViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    DueDate = x.DueDate,
                    TransactionId = x.TransactionId,
                    AccountId = x.AccountId
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (viewModel == null)
            {
                _logger.LogWarning($"User id {userId} attempted to access unknown bill {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckAccountAccessAsync(userId, viewModel.AccountId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to access bill {id}");
                return Forbid();
            }

            // Log the operation
            _logger.LogInformation($"User {userId} read bill {viewModel.Id}");

            return Ok(viewModel);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<BillViewModel>> PutBill(Guid id, BillViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null || id != viewModel.Id)
            {
                return BadRequest();
            }

            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            var bill = await _context.Bills
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (bill == null)
            {
                _logger.LogWarning($"User id {userId} attempted to update unknown bill {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, bill.Account.Tenant.Id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to update bill {id}");
                return Forbid();
            }

            // Update the bill
            bill.Name = viewModel.Name;
            bill.Amount = viewModel.Amount;
            bill.Currency = viewModel.Currency;
            bill.DueDate = viewModel.DueDate;
            bill.UpdatedUser = userId;
            bill.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(bill);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Log the operation
            _logger.LogInformation($"User {userId} updated bill {bill.Id}");

            return Ok(new BillViewModel
            {
                Id = bill.Id,
                Name = bill.Name,
                Amount = bill.Amount,
                Currency = bill.Currency,
                DueDate = bill.DueDate,
                TransactionId = bill.TransactionId,
                AccountId = bill.AccountId
            });
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<BillViewModel>> PostBill(BillViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null)
            {
                return BadRequest();
            }

            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified account
            var account = await _context.Accounts
                .Include(x => x.Tenant)
                .FirstOrDefaultAsync(x => x.Id == viewModel.AccountId)
                .ConfigureAwait(false);
            if (account == null)
            {
                _logger.LogWarning("Could not find account for bill schedule");
                return BadRequest("Invalid account");
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, account.Tenant.Id)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to create bills");
                return Forbid();
            }

            // Create the new bill
            var bill = new Bill
            {
                Name = viewModel.Name,
                Amount = viewModel.Amount,
                DueDate = viewModel.DueDate,
                Account = account,
                CreatedUser = userId
            };

            // Optional currency specification
            if (!string.IsNullOrEmpty(viewModel.Currency))
            {
                bill.Currency = viewModel.Currency;
            }

            await _context.Bills.AddAsync(bill)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Log the operation
            _logger.LogInformation($"User {userId} created bill {bill.Id}");

            viewModel.Id = bill.Id;

            return CreatedAtAction("GetBill", new {id = viewModel.Id}, viewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteBill(Guid id)
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (userId == Guid.Empty)
            {
                return Forbid();
            }

            // Find the specified bill
            var bill = await _context.Bills
                // Include the account
                .Include(x => x.Account)
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (bill == null)
            {
                _logger.LogWarning($"User id {userId} attempted to delete unknown bill {id}");
                return NotFound();
            }

            // Ensure the user is authorized at the requested level
            var userAuthorized = await _validator.CheckTenantAccessAsync(userId, bill.Account.TenantId)
                .ConfigureAwait(false);
            if (!userAuthorized)
            {
                _logger.LogWarning($"User id {userId} was not authorized to delete bill {id}");
                return Forbid();
            }

            // Disable the bill
            bill.Disabled = true;
            bill.DisabledUser = userId;
            bill.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(bill);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}