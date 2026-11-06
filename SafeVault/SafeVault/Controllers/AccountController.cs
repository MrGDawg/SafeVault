using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SafeVault.Data;
using SafeVault.Models;
using SafeVault.Services;

namespace SafeVault.Controllers
{
    /// <summary>
    /// Handles SafeVault authentication requests.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly SafeVaultDatabase _database;
        private readonly PasswordService _passwordService;

        /// <summary>
        /// Initializes the controller with the services required
        /// to retrieve accounts and verify passwords.
        /// </summary>
        public AccountController(
            SafeVaultDatabase database,
            PasswordService passwordService)
        {
            _database = database;
            _passwordService = passwordService;
        }

        /// <summary>
        /// Displays the SafeVault login form.
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        /// <summary>
        /// Verifies submitted credentials and establishes an
        /// authenticated session when the credentials are valid.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user =
                _database.GetUserByUsername(model.Username);

            // Return the same error for an unknown username and an
            // incorrect password so the response does not reveal
            // which part of the submitted credentials was invalid.
            if (user is null ||
                !_passwordService.VerifyPassword(
                    user,
                    model.Password))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid username or password.");

                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction(
                nameof(LoginSuccess));
        }
        /// <summary>
        /// Displays confirmation that the current request contains
        /// an authenticated SafeVault identity.
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult LoginSuccess()
        {
            return View();
        }

        /// <summary>
        /// Displays a friendly response when an authenticated user
        /// attempts to access a resource without the required permission.
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}