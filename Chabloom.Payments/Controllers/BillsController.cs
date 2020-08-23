// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chabloom.Payments.Data;
using Chabloom.Payments.Models;
using Chabloom.Payments.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Payments.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BillsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BillsController> _logger;

        public BillsController(ApplicationDbContext context, ILogger<BillsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<BillViewModel>>> GetBills(Guid? tenantId)
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("User attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"User sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // TODO: Query tenant for access

            List<BillViewModel> bills;
            if (tenantId == null)
            {
                // Find all bills the user has access to
                bills = await _context.Bills
                    .Include(x => x.Account)
                    .ThenInclude(x => x.Users)
                    .Where(x => x.Account.Users.Select(y => y.UserId).Contains(userId))
                    .Where(x => !x.Disabled)
                    .Select(x => new BillViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Amount = x.Amount,
                        DueDate = x.DueDate,
                        Account = x.Account.Id
                    })
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                // Find all bills the user has access to
                bills = await _context.Bills
                    .Include(x => x.Account)
                    .ThenInclude(x => x.Tenant)
                    .Include(x => x.Account)
                    .ThenInclude(x => x.Users)
                    .Where(x => x.Account.Users.Select(y => y.UserId).Contains(userId))
                    .Where(x => !x.Disabled)
                    .Where(x => x.Account.Tenant.Id == tenantId)
                    .Select(x => new BillViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Amount = x.Amount,
                        DueDate = x.DueDate,
                        Account = x.Account.Id
                    })
                    .ToListAsync()
                    .ConfigureAwait(false);
            }

            return Ok(bills);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<BillViewModel>> GetBill(Guid id)
        {
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("User attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"User sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // TODO: Query tenant for access

            // Find the specified bill if the user has access to it
            var bill = await _context.Bills
                .Include(x => x.Account)
                .ThenInclude(x => x.Users)
                .Where(x => x.Account.Users.Select(y => y.UserId).Contains(userId))
                .Where(x => !x.Disabled)
                .Select(x => new BillViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Amount = x.Amount,
                    DueDate = x.DueDate,
                    Account = x.Account.Id
                })
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (bill == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown bill {id}");
                return NotFound();
            }

            return Ok(bill);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
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

            if (viewModel == null || id != viewModel.Id)
            {
                return BadRequest();
            }

            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("User attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"User sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // Find the specified bill
            var bill = await _context.Bills
                .Include(x => x.Account)
                .ThenInclude(x => x.Tenant)
                .ThenInclude(x => x.Users)
                .ThenInclude(x => x.Role)
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (bill == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown bill {id}");
                return NotFound();
            }

            // Ensure the current user belongs to the tenant
            if (!bill.Account.Tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {bill.Account.Tenant.Id}");
                return Forbid();
            }

            // Ensure the current user is a tenant admin or manager
            var tenantUser = bill.Account.Tenant.Users
                .FirstOrDefault(x => x.UserId == userId);
            if (tenantUser != null &&
                tenantUser.Role.Name != "Admin" &&
                tenantUser.Role.Name != "Manager")
            {
                _logger.LogWarning($"User id {userId} did not have permissions to update bill for tenant {bill.Account.Tenant.Id}");
                return Forbid();
            }

            // Update the bill
            bill.Name = viewModel.Name;
            bill.Amount = viewModel.Amount;
            bill.DueDate = viewModel.DueDate;
            bill.UpdatedUser = userId;
            bill.UpdatedTimestamp = DateTimeOffset.UtcNow;

            _context.Update(bill);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
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

            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("User attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"User sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            var bill = new Bill
            {
                Name = viewModel.Name,
                Amount = viewModel.Amount,
                DueDate = viewModel.DueDate,
                Account = await _context.Accounts
                    .Include(x => x.Tenant)
                    .ThenInclude(x => x.Users)
                    .ThenInclude(x => x.Role)
                    .FirstOrDefaultAsync(x => x.Id == viewModel.Account)
                    .ConfigureAwait(false),
                CreatedUser = userId,
                UpdatedUser = userId,
                DisabledUser = userId
            };

            // Ensure the account was found
            if (bill.Account == null)
            {
                _logger.LogWarning($"Specified account {viewModel.Account} could not be found");
                return BadRequest();
            }

            // Ensure the current user belongs to the tenant
            if (!bill.Account.Tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {bill.Account.Tenant.Id}");
                return Forbid();
            }

            // Ensure the current user is a tenant admin or manager
            var tenantUser = bill.Account.Tenant.Users
                .FirstOrDefault(x => x.UserId == userId);
            if (tenantUser != null &&
                tenantUser.Role.Name != "Admin" &&
                tenantUser.Role.Name != "Manager")
            {
                _logger.LogWarning($"User id {userId} did not have permissions to create bill for tenant {bill.Account.Tenant.Id}");
                return Forbid();
            }

            await _context.Bills.AddAsync(bill)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

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
            // Get the current user sid
            var sid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(sid))
            {
                _logger.LogWarning("User attempted call without an sid");
                return Forbid();
            }

            // Ensure the user id can be parsed
            if (!Guid.TryParse(sid, out var userId))
            {
                _logger.LogWarning($"User sid {sid} could not be parsed as Guid");
                return Forbid();
            }

            // Find the specified bill
            var bill = await _context.Bills
                .Include(x => x.Account)
                .ThenInclude(x => x.Tenant)
                .ThenInclude(x => x.Users)
                .ThenInclude(x => x.Role)
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (bill == null)
            {
                // Return 404 even if the item exists to prevent leakage of items
                _logger.LogWarning($"User id {userId} attempted to access unknown bill {id}");
                return NotFound();
            }

            // Ensure the current user belongs to the tenant
            if (!bill.Account.Tenant.Users
                .Select(x => x.UserId)
                .Contains(userId))
            {
                _logger.LogWarning($"User id {userId} did not belong to tenant {bill.Account.Tenant.Id}");
                return Forbid();
            }

            // Ensure the current user is a tenant admin or manager
            var tenantUser = bill.Account.Tenant.Users
                .FirstOrDefault(x => x.UserId == userId);
            if (tenantUser != null &&
                tenantUser.Role.Name != "Admin" &&
                tenantUser.Role.Name != "Manager")
            {
                _logger.LogWarning($"User id {userId} did not have permissions to disable bill for tenant {bill.Account.Tenant.Id}");
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