namespace SafeVault.Models
{
    /// <summary>
    /// Represents a SafeVault user account used for authentication
    /// and role-based authorization.
    /// </summary>
    public class UserAccount
    {
        /// <summary>
        /// Gets or sets the database identifier for the account.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the account's unique username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address associated with the account.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the protected representation of the password.
        /// The original plaintext password is never stored.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the role used when making authorization decisions.
        /// </summary>
        public string Role { get; set; } = "user";
    }
}