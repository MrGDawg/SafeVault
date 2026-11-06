using Microsoft.AspNetCore.Identity;
using SafeVault.Models;

namespace SafeVault.Services
{
    /// <summary>
    /// Provides password hashing and verification operations for
    /// SafeVault user accounts.
    /// </summary>
    public class PasswordService
    {
        private readonly IPasswordHasher<UserAccount> _passwordHasher;

        /// <summary>
        /// Initializes the service with ASP.NET Core's password hasher.
        /// </summary>
        public PasswordService(
            IPasswordHasher<UserAccount> passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Creates a protected password hash suitable for database storage.
        /// </summary>
        public string HashPassword(
            UserAccount user,
            string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

        /// <summary>
        /// Determines whether a supplied password matches the stored hash.
        /// </summary>
        public bool VerifyPassword(
            UserAccount user,
            string providedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                providedPassword);

            return result is PasswordVerificationResult.Success
                or PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}