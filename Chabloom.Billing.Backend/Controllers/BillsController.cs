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

        public BillsController(ApplicationDbContext context, ILogger<BillsController> logger, IValidator validator)
        {
            _context = context;
            _logger = logger;
            _validator = validator;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetBills(Guid accountId)
        {
            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            // Ensure the user is authorized at the requested level
            var (roleValid, roleResult) = await ValidateRoleAccessAsync(userId.Value, tenantId.Value);
            if (!roleValid)
            {
                return roleResult;
            }

            // Get all bills
            var bills = await _context.Bills
                .Include(x => x.Account)
                .Where(x => x.AccountId == accountId)
                .Where(x => x.Account.TenantId == tenantId)
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .ToListAsync();
            if (bills == null)
            {
                return Ok(new List<BillViewModel>());
            }

            var viewModels = bills
                .Select(x => new BillViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    CurrencyId = x.CurrencyId,
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
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBill(Guid id)
        {
            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            // Find the specified bill
            var bill = await _context.Bills
                .FindAsync(id);
            if (bill == null)
            {
                return NotFound();
            }

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(bill, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            var retViewModel = new BillViewModel
            {
                Id = bill.Id,
                Name = bill.Name,
                Amount = bill.Amount,
                CurrencyId = bill.CurrencyId,
                DueDate = bill.DueDate,
                TransactionId = bill.TransactionId,
                AccountId = bill.AccountId
            };

            return Ok(retViewModel);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutBill(Guid id, BillViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            // Find the specified bill
            var bill = await _context.Bills
                .FindAsync(id);
            if (bill == null)
            {
                return NotFound();
            }

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(bill, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            // Ensure the user is authorized at the requested level
            var (roleValid, roleResult) = await ValidateRoleAccessAsync(userId.Value, tenantId.Value);
            if (!roleValid)
            {
                return roleResult;
            }

            bill.Name = viewModel.Name;
            bill.Amount = viewModel.Amount;
            bill.CurrencyId = viewModel.CurrencyId;
            bill.DueDate = viewModel.DueDate;
            bill.UpdatedUser = userId.Value;
            bill.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(bill);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} updated bill {bill.Id}");

            var retViewModel = new BillViewModel
            {
                Id = bill.Id,
                Name = bill.Name,
                Amount = bill.Amount,
                CurrencyId = bill.CurrencyId,
                DueDate = bill.DueDate,
                TransactionId = bill.TransactionId,
                AccountId = bill.AccountId
            };

            return Ok(retViewModel);
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> PostBill(BillViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            var bill = new Bill
            {
                Name = viewModel.Name,
                Amount = viewModel.Amount,
                CurrencyId = viewModel.CurrencyId,
                DueDate = viewModel.DueDate,
                AccountId = viewModel.AccountId,
                CreatedUser = userId.Value
            };

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(bill, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            // Ensure the user is authorized at the requested level
            var (roleValid, roleResult) = await ValidateRoleAccessAsync(userId.Value, tenantId.Value);
            if (!roleValid)
            {
                return roleResult;
            }

            await _context.AddAsync(bill);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} created bill {bill.Id}");

            var retViewModel = new BillViewModel
            {
                Id = bill.Id,
                Name = bill.Name,
                Amount = bill.Amount,
                CurrencyId = bill.CurrencyId,
                DueDate = bill.DueDate,
                TransactionId = bill.TransactionId,
                AccountId = bill.AccountId
            };

            return Ok(retViewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteBill(Guid id)
        {
            // Get the user and tenant id
            var (userId, tenantId, userTenantResult) = await GetUserTenantAsync();
            if (!userId.HasValue || !tenantId.HasValue)
            {
                return userTenantResult;
            }

            // Find the specified bill
            var bill = await _context.Bills
                .FindAsync(id);
            if (bill == null)
            {
                return NotFound();
            }

            // Validate that the endpoint is called from the correct tenant
            var (tenantValid, tenantResult) = await ValidateTenantAsync(bill, tenantId.Value);
            if (!tenantValid)
            {
                return tenantResult;
            }

            // Ensure the user is authorized at the requested level
            var (roleValid, roleResult) = await ValidateRoleAccessAsync(userId.Value, tenantId.Value);
            if (!roleValid)
            {
                return roleResult;
            }

            // Disable the bill
            bill.Disabled = true;
            bill.DisabledUser = userId.Value;
            bill.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(bill);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} disabled bill {bill.Id}");

            return NoContent();
        }

        private async Task<Tuple<Guid?, Guid?, IActionResult>> GetUserTenantAsync()
        {
            // Get the user id
            var userId = _validator.GetUserId(User);
            if (!userId.HasValue)
            {
                return new Tuple<Guid?, Guid?, IActionResult>(null, null, Forbid());
            }

            // Get the current tenant id
            var tenantId = await _validator.GetTenantIdAsync(Request);
            if (!tenantId.HasValue)
            {
                return new Tuple<Guid?, Guid?, IActionResult>(null, null, Forbid());
            }

            return new Tuple<Guid?, Guid?, IActionResult>(userId, tenantId, Forbid());
        }

        private async Task<Tuple<bool, IActionResult>> ValidateTenantAsync(Bill bill, Guid tenantId)
        {
            // Find the specified account
            var account = await _context.Accounts
                .FindAsync(bill.AccountId);
            if (account == null)
            {
                return new Tuple<bool, IActionResult>(false, NotFound());
            }

            // Ensure the user is calling this endpoint from the correct tenant
            if (account.TenantId != tenantId)
            {
                return new Tuple<bool, IActionResult>(false, Forbid());
            }

            return new Tuple<bool, IActionResult>(true, Ok());
        }

        private async Task<Tuple<bool, IActionResult>> ValidateRoleAccessAsync(Guid userId, Guid tenantId)
        {
            // Ensure the user is authorized at the requested level
            var userRoles = await _validator.GetTenantRolesAsync(userId, tenantId);
            if (!userRoles.Contains("Admin") &&
                !userRoles.Contains("Manager"))
            {
                _logger.LogWarning($"User id {userId} was not authorized to perform requested operation");
                return new Tuple<bool, IActionResult>(false, Forbid());
            }

            return new Tuple<bool, IActionResult>(true, Ok());
        }
    }
}