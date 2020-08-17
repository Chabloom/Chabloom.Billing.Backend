// Copyright 2020 Chabloom LC. All rights reserved.

using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Chabloom.Payments.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly IClientRequestParametersProvider _clientRequestParametersProvider;
        private readonly ILogger<ConfigurationController> _logger;

        public ConfigurationController(IClientRequestParametersProvider clientRequestParametersProvider,
            ILogger<ConfigurationController> logger)
        {
            _clientRequestParametersProvider = clientRequestParametersProvider;
            _logger = logger;
        }

        [HttpGet("_configuration/{clientId}")]
        public IActionResult GetClientRequestParameters([FromRoute] string clientId)
        {
            var parameters = _clientRequestParametersProvider.GetClientParameters(HttpContext, clientId);
            return Ok(parameters);
        }
    }
}