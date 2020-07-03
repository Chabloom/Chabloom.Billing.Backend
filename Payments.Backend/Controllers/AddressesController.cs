// Copyright 2020 Chabloom LC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Backend.Data;
using Payments.Backend.Models;

namespace Payments.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly PaymentServiceDbContext _context;

        public AddressesController(PaymentServiceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AddressViewModel>>> GetAddresses()
        {
            return await _context.Addresses
                .Select(x => x.ToViewModel())
                .ToListAsync()
                .ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AddressViewModel>> GetAddress(Guid id)
        {
            var address = await _context.Addresses.FindAsync(id)
                .ConfigureAwait(false);

            if (address == null)
            {
                return NotFound();
            }

            return address.ToViewModel();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAddress(Guid id, AddressViewModel viewModel)
        {
            if (viewModel == null || id != viewModel.Id)
            {
                return BadRequest();
            }

            var address = await _context
                .Addresses
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (address == null)
            {
                return NotFound();
            }

            address = viewModel.ToModel();

            _context.Update(address);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<AddressViewModel>> PostAddress(AddressViewModel address)
        {
            if (address == null)
            {
                return BadRequest();
            }

            await _context.Addresses.AddAsync(address.ToModel())
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return CreatedAtAction("GetAddress", new {id = address.Id}, address);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<AddressViewModel>> DeleteAddress(Guid id)
        {
            var address = await _context.Addresses.FindAsync(id)
                .ConfigureAwait(false);
            if (address == null)
            {
                return NotFound();
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return address.ToViewModel();
        }
    }
}