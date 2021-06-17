// Copyright 2020-2021 Chabloom LC. All rights reserved.

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chabloom.Billing.Backend.Data;
using Chabloom.Billing.Backend.Models.Tenants;
using Chabloom.Billing.Backend.Services;
using Chabloom.Billing.Backend.ViewModels.Auth;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chabloom.Billing.Backend.Controllers.Auth
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IUserClaimsPrincipalFactory<TenantUser> _claimsPrincipalFactory;
        private readonly ApplicationDbContext _context;
        private readonly EmailSender _emailSender;
        private readonly IIdentityServerInteractionService _interactionService;
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager<TenantUser> _signInManager;
        private readonly SmsSender _smsSender;
        private readonly UserManager<TenantUser> _userManager;
        private readonly IValidator _validator;

        public AuthController(
            IUserClaimsPrincipalFactory<TenantUser> claimsPrincipalFactory,
            ApplicationDbContext context,
            EmailSender emailSender,
            IIdentityServerInteractionService interactionService,
            ILogger<AuthController> logger,
            SignInManager<TenantUser> signInManager,
            SmsSender smsSender,
            UserManager<TenantUser> userManager,
            IValidator validator)
        {
            _claimsPrincipalFactory = claimsPrincipalFactory;
            _context = context;
            _emailSender = emailSender;
            _interactionService = interactionService;
            _logger = logger;
            _signInManager = signInManager;
            _smsSender = smsSender;
            _userManager = userManager;
            _validator = validator;
        }

        [HttpPost("SignIn")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AccountSignInAsync([FromBody] SignInViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Get the current tenant id
            var tenantId = await _validator.GetTenantIdAsync(Request);
            if (!tenantId.HasValue)
            {
                return Forbid();
            }

            // Find the user specified in the login request
            var user = await _context.Users
                .FindAsync(viewModel.Username.ToUpper(), tenantId);
            if (user == null)
            {
                return NotFound();
            }

            // Application sign in
            var result = await _signInManager.PasswordSignInAsync(user, viewModel.Password, viewModel.Persist,
                user.AccessFailedCount > 3);
            if (!result.Succeeded)
            {
                _logger.LogWarning($"User {user.Id} sign in failure");
                return Unauthorized();
            }

            // Get the user claims principal
            var claimsPrincipal = await _claimsPrincipalFactory.CreateAsync(user);

            // Cookie sign in
            await HttpContext.SignInAsync(claimsPrincipal);

            _logger.LogInformation($"User {user.Id} sign in success");

            return Ok();
        }

        [HttpPost("SignOut/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AccountSignOutAsync([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            // Get the logout context
            var context = await _interactionService.GetLogoutContextAsync(id);

            // Cookie sign out
            await HttpContext.SignOutAsync();

            // Application sign out
            await _signInManager.SignOutAsync();

            _logger.LogInformation($"Sign out {id} success");

            return Ok(context.PostLogoutRedirectUri);
        }

        [HttpPost("Register")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AccountRegisterAsync([FromBody] RegisterViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Get the current tenant id
            var tenantId = await _validator.GetTenantIdAsync(Request);
            if (!tenantId.HasValue)
            {
                return Forbid();
            }

            // Initialize the user specified in the register request
            var user = new TenantUser
            {
                UserName = viewModel.Username,
                Email = viewModel.Email,
                PhoneNumber = viewModel.PhoneNumber,
                TenantId = tenantId.Value
            };

            // Create the user specified in the register request
            var result = await _userManager.CreateAsync(user, viewModel.Password);
            if (!result.Succeeded)
            {
                return BadRequest();
            }

            // Add the user claims
            await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, viewModel.Name));

            // Send email confirmation link
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            if (!string.IsNullOrEmpty(emailToken))
            {
                const string subject = "Confirm your Chabloom account";
                await _emailSender.SendEmailAsync(user.Email, subject, emailToken);
            }

            // Send sms confirmation link
            var smsToken = await _userManager.GenerateChangePhoneNumberTokenAsync(user, viewModel.PhoneNumber);
            if (!string.IsNullOrEmpty(smsToken))
            {
                await _smsSender.SendSmsAsync(user.PhoneNumber, smsToken);
            }

            _logger.LogInformation($"User {user.Id} registration success");

            return Ok();
        }

        [HttpGet("Error/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AccountErrorAsync([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            // Get the error context
            var context = await _interactionService.GetErrorContextAsync(id);

            // Populate the ret view model
            var retViewModel = new ErrorViewModel
            {
                Error = context.Error,
                ErrorDescription = context.ErrorDescription,
                RedirectUri = context.RedirectUri
            };

            return Ok(retViewModel);
        }
    }
}