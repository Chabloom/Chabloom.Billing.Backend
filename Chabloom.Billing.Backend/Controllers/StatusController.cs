// Copyright 2020-2021 Chabloom LC. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chabloom.Billing.Backend.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(200)]
        public IActionResult GetStatus()
        {
            // Return success status code
            return Ok();
        }
    }
}