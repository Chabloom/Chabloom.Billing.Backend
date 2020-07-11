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
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class PartitionsController : ControllerBase
    {
        private readonly PaymentServiceDbContext _context;

        public PartitionsController(PaymentServiceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<PartitionViewModel>>> GetPartitions()
        {
            return await _context.Partitions
                .Select(x => new PartitionViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Enabled = x.Enabled
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PartitionViewModel>> GetPartition(Guid id)
        {
            var partition = await _context.Partitions
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (partition == null)
            {
                return NotFound();
            }

            return new PartitionViewModel
            {
                Id = partition.Id,
                Name = partition.Name,
                Enabled = partition.Enabled
            };
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutPartition(Guid id, PartitionViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null || id != viewModel.Id)
            {
                return BadRequest();
            }

            var partition = await _context
                .Partitions
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);
            if (partition == null)
            {
                return NotFound();
            }

            partition.Name = viewModel.Name;
            partition.Enabled = viewModel.Enabled;

            _context.Update(partition);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PartitionViewModel>> PostPartition(PartitionViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (viewModel == null)
            {
                return BadRequest();
            }

            var partition = new Partition
            {
                Name = viewModel.Name,
                Enabled = viewModel.Enabled
            };

            await _context.Partitions.AddAsync(partition)
                .ConfigureAwait(false);
            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            viewModel.Id = partition.Id;

            return CreatedAtAction("GetPartition", new {id = viewModel.Id}, viewModel);
        }
    }
}