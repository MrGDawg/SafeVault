using System.ComponentModel.DataAnnotations;

namespace SafeVault.Models
{
    /// <summary>
    /// Represents user information submitted through the SafeVault form.
    /// Validation attributes define acceptable input before the controller
    /// passes the data to the data-access layer.
    /// </summary>
    public class UserInputModel
    {
        /// <summary>
        /// Gets or sets the validated username.
        /// </summary>
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(
            50,
            MinimumLength = 2,
            ErrorMessage = "Username must be between 2 and 50 characters.")]
        [RegularExpression(
            @"^[a-zA-Z0-9 ._-]+$",
            ErrorMessage =
                "Username may contain only letters, numbers, spaces, periods, underscores, and hyphens.")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the validated email address.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [StringLength(
            254,
            ErrorMessage = "Email cannot exceed 254 characters.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;
    }
}