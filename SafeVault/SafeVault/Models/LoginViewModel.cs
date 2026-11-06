using System.ComponentModel.DataAnnotations;

namespace SafeVault.Models
{
    /// <summary>
    /// Represents credentials submitted through the SafeVault login form.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Gets or sets the username supplied during login.
        /// </summary>
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(
            50,
            ErrorMessage = "Username cannot exceed 50 characters.")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password supplied during login.
        /// The plaintext password is used only for verification
        /// and is never stored by the application.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}