// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TrabalhoES2.Models;

namespace TrabalhoES2.Areas.Identity.Pages.Account
{
    [IgnoreAntiforgeryToken]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly UserManager<Utilizador> _userManager; // Adicione esta linha
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<Utilizador> signInManager, 
            UserManager<Utilizador> userManager, // Adicione este parâmetro
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager; // Adicione esta atribuição
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            base.OnPageHandlerExecuting(context);
            // If testmode=1, disable antiforgery validation for this request
            if (Request.Query["testmode"] == "1")
            {
                // Remove antiforgery validation filter for this request
                var filters = context.Filters;
                for (int i = filters.Count - 1; i >= 0; i--)
                {
                    if (filters[i] is AutoValidateAntiforgeryTokenAttribute)
                    {
                        filters.RemoveAt(i);
                    }
                }
            }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (Request.Query["testmode"] == "1")
            {
                Response.Cookies.Append("testmode", "1", new CookieOptions { Path = "/" });
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            // Preserve testmode=1 in redirect if present
            bool isTestMode = Request.Query["testmode"] == "1";
            if (isTestMode && !returnUrl.Contains("testmode=1"))
            {
                if (returnUrl.Contains("?"))
                    returnUrl += "&testmode=1";
                else
                    returnUrl += "?testmode=1";
            }
            if (isTestMode)
            {
                Response.Cookies.Append("testmode", "1", new CookieOptions { Path = "/" });
            }
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            string postedEmail = Input?.Email ?? "(none)";
            string postedPassword = (Input?.Password != null && Input.Password.Length > 0) ? "***" : "(none)";
            string modelStateErrors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            string isAuth = User?.Identity?.IsAuthenticated == true ? "true" : "false";

            // --- DEBUG: Log user info and password check ---
            Utilizador user = null;
            bool passwordValid = false;
            string userPasswordHash = null;
            if (!string.IsNullOrEmpty(Input?.Email))
            {
                user = await _userManager.FindByEmailAsync(Input.Email);
                if (user != null && !string.IsNullOrEmpty(Input.Password))
                {
                    passwordValid = await _userManager.CheckPasswordAsync(user, Input.Password);
                    userPasswordHash = user.PasswordHash;
                }
            }

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                TempData["LoginResult"] = result.Succeeded ? "Success" : result.ToString();
                TempData["LastLoginDebug"] = $"[POST] Email: '{postedEmail}', Password: '{postedPassword}', ModelState: {modelStateErrors}, Auth: {isAuth}, User: {{Id={user?.Id}, Email={user?.Email}, EmailConfirmed={user?.EmailConfirmed}, IsBlocked={user?.IsBlocked}, IsDeleted={user?.IsDeleted}, PasswordHash={(userPasswordHash != null ? userPasswordHash.Substring(0, 20) + "..." : "(null)")}, PasswordValid={passwordValid}}}, SignInResult: {{Succeeded={result.Succeeded}, IsLockedOut={result.IsLockedOut}, RequiresTwoFactor={result.RequiresTwoFactor}, IsNotAllowed={result.IsNotAllowed}}}";
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl ?? Url.Content("~/"));
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    TempData["ErrorMessage"] = "User account locked out.";
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid login attempt.";
                }
            }
            else
            {
                TempData["LastLoginDebug"] = $"[POST] Email: '{postedEmail}', Password: '{postedPassword}', ModelState: {modelStateErrors}, Auth: {isAuth}, User: {{Id={user?.Id}, Email={user?.Email}, EmailConfirmed={user?.EmailConfirmed}, IsBlocked={user?.IsBlocked}, IsDeleted={user?.IsDeleted}, PasswordHash={(userPasswordHash != null ? userPasswordHash.Substring(0, 20) + "..." : "(null)")}, PasswordValid={passwordValid}}}, ERROR: ModelState invalid";
            }
            return Page();
        }
    }
}