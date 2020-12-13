// Copyright 2020 Chabloom LC. All rights reserved.

using System.Linq;
using System.Threading.Tasks;
using Chabloom.Payments.Data;
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
    public class QuickTransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuickTransactionController> _logger;

        public QuickTransactionController(ApplicationDbContext context, ILogger<QuickTransactionController> logger)
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

            var payment = await _context.Payments
                // Don't include deleted items
                .Where(x => !x.Disabled)
                .FirstOrDefaultAsync(x => x.Id == viewModel.PaymentId)
                .ConfigureAwait(false);
            if (payment == null)
            {
                _logger.LogWarning($"User attempted to update unknown payment {viewModel.PaymentId}");
                return NotFound();
            }

            // Update the payment
            payment.TransactionId = viewModel.TransactionId;

            _context.Update(payment);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            // Log the operation
            _logger.LogInformation($"User made quick transaction for payment {payment.Id}");

            return NoContent();
        }
    }
}