// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System.Linq;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
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
    public class QuickTransactionController : Controller
    {
        private readonly BillingDbContext _context;
        private readonly ILogger<QuickTransactionController> _logger;

        public QuickTransactionController(BillingDbContext context, ILogger<QuickTransactionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<QuickTransactionViewModel>> PostQuickTransaction(
            QuickTransactionViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null)
            {
                return BadRequest();
            }

            var bill = await _context.Bills
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == viewModel.BillId)
                .ConfigureAwait(false);
            if (bill == null)
            {
                _logger.LogWarning($"User attempted to update unknown bill {viewModel.BillId}");
                return NotFound();
            }

            // Update the bill
            bill.TransactionId = viewModel.TransactionId;

            _context.Update(bill);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Log the operation
            _logger.LogInformation($"User made quick transaction for bill {bill.Id}");

            return NoContent();
        }
    }
}