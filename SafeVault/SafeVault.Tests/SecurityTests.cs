using Microsoft.AspNetCore.Authorization;
using SafeVault.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SafeVault.Services;
using System.ComponentModel.DataAnnotations;
using SafeVault.Models;

namespace SafeVault.Tests
{
    //// <summary>
    /// Unit-level security tests for SafeVault input validation,
    /// password handling, authentication models, and role claims.
    /// </summary>
    public class SecurityTests
    {
        /// <summary>
        /// Runs the Data Annotation validation rules defined on a model
        /// and returns the resulting validation errors.
        /// </summary>
        private static List<ValidationResult> ValidateModel<TModel>(
            TModel model)
            where TModel : class
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);

            Validator.TryValidateObject(
                model,
                context,
                results,
                validateAllProperties: true);

            return results;
        }

        /// <summary>
        /// Creates the password service used by the authentication tests.
        /// </summary>
        private static PasswordService CreatePasswordService()
        {
            var options = Options.Create(
                new PasswordHasherOptions());

            var passwordHasher =
                new PasswordHasher<UserAccount>(options);

            return new PasswordService(passwordHasher);
        }

        /// <summary>
        /// Confirms that normal SafeVault input passes validation.
        ///
        /// This test gives us a baseline: security controls should
        /// reject malicious input without rejecting legitimate input.
        /// </summary>
        [Fact]
        public void ValidInput_PassesValidation()
        {
            // Arrange
            var model = new UserInputModel
            {
                Username = "Paul_Smith",
                Email = "paul@example.com"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Empty(results);
        }

        /// <summary>
        /// Simulates an XSS attack placed in the Username field.
        /// The username allowlist should reject the script markup.
        /// </summary>
        [Fact]
        public void XssAttempt_IsRejected()
        {
            // Arrange
            var model = new UserInputModel
            {
                Username = "<script>alert('XSS')</script>",
                Email = "paul@example.com"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.NotEmpty(results);
        }

        /// <summary>
        /// Simulates a SQL-injection-style value supplied
        /// through the Username field.
        ///
        /// Input validation should reject the characters before
        /// the controller attempts the database operation.
        /// </summary>
        [Fact]
        public void SqlInjectionAttempt_IsRejected()
        {
            // Arrange
            var model = new UserInputModel
            {
                Username = "' OR 1=1 --",
                Email = "paul@example.com"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.NotEmpty(results);
        }

        /// <summary>
        /// Confirms that an empty username and email address
        /// fail the Required validation rules.
        /// </summary>
        [Fact]
        public void EmptyInput_IsRejected()
        {
            // Arrange
            var model = new UserInputModel
            {
                Username = string.Empty,
                Email = string.Empty
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.NotEmpty(results);
        }

        /// <summary>
        /// Confirms that an improperly formatted email
        /// address is rejected.
        /// </summary>
        [Fact]
        public void InvalidEmail_IsRejected()
        {
            // Arrange
            var model = new UserInputModel
            {
                Username = "Paul_Smith",
                Email = "not-an-email-address"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.NotEmpty(results);
        }

        /// <summary>
        /// Confirms that a password is converted into a protected value
        /// rather than being retained as plaintext.
        /// </summary>
        [Fact]
        public void HashPassword_DoesNotReturnPlaintextPassword()
        {
            // Arrange
            var passwordService = CreatePasswordService();

            var user = new UserAccount
            {
                Username = "Paul_Smith",
                Email = "paul@example.com"
            };

            const string password = "SafeVault-Test-Password-2026!";

            // Act
            var passwordHash =
                passwordService.HashPassword(user, password);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(passwordHash));
            Assert.NotEqual(password, passwordHash);
        }

        /// <summary>
        /// Confirms that the correct password matches its stored hash.
        /// </summary>
        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var passwordService = CreatePasswordService();

            var user = new UserAccount
            {
                Username = "Paul_Smith",
                Email = "paul@example.com"
            };

            const string password = "SafeVault-Test-Password-2026!";

            user.PasswordHash =
                passwordService.HashPassword(user, password);

            // Act
            var isValid =
                passwordService.VerifyPassword(user, password);

            // Assert
            Assert.True(isValid);
        }

        /// <summary>
        /// Confirms that an incorrect password does not match
        /// the stored password hash.
        /// </summary>
        [Fact]
        public void VerifyPassword_IncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var passwordService = CreatePasswordService();

            var user = new UserAccount
            {
                Username = "Paul_Smith",
                Email = "paul@example.com"
            };

            user.PasswordHash =
                passwordService.HashPassword(
                    user,
                    "SafeVault-Test-Password-2026!");

            // Act
            var isValid =
                passwordService.VerifyPassword(
                    user,
                    "Incorrect-Password!");

            // Assert
            Assert.False(isValid);
        }
        /// <summary>
        /// Confirms that an authenticated identity containing the admin
        /// role claim is recognized as belonging to the admin role.
        /// </summary>
        [Fact]
        public void AdminIdentity_IsRecognizedAsAdmin()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "SafeVaultAdmin"),
                new(ClaimTypes.Role, "admin")
            };

            var identity = new ClaimsIdentity(
                claims,
                authenticationType: "TestAuthentication");

            var user = new ClaimsPrincipal(identity);

            // Act
            var isAdmin = user.IsInRole("admin");

            // Assert
            Assert.True(user.Identity?.IsAuthenticated);
            Assert.True(isAdmin);
        }
        /// <summary>
        /// Confirms that an authenticated user-role identity is not
        /// incorrectly recognized as belonging to the admin role.
        /// </summary>
        [Fact]
        public void UserIdentity_IsNotRecognizedAsAdmin()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "SafeVaultUser"),
                new(ClaimTypes.Role, "user")
            };

            var identity = new ClaimsIdentity(
                claims,
                authenticationType: "TestAuthentication");

            var user = new ClaimsPrincipal(identity);

            // Act
            var isAdmin = user.IsInRole("admin");

            // Assert
            Assert.True(user.Identity?.IsAuthenticated);
            Assert.False(isAdmin);
        }
        /// <summary>
        /// Confirms that the AdminController explicitly requires
        /// membership in the admin role.
        /// </summary>
        [Fact]
        public void AdminController_RequiresAdminRole()
        {
            // Arrange
            var controllerType = typeof(AdminController);

            // Act
            var authorizeAttribute =
                controllerType
                    .GetCustomAttributes(
                        typeof(AuthorizeAttribute),
                        inherit: true)
                    .Cast<AuthorizeAttribute>()
                    .SingleOrDefault();

            // Assert
            Assert.NotNull(authorizeAttribute);
            Assert.Equal("admin", authorizeAttribute.Roles);
        }
        /// <summary>
        /// Confirms that a login attempt without a username
        /// fails the login model's validation rules.
        /// </summary>
        [Fact]
        public void LoginWithoutUsername_IsRejected()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Username = string.Empty,
                Password = "Test-Password-2026!"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(
                results,
                result =>
                    result.MemberNames.Contains(
                        nameof(LoginViewModel.Username)));
        }
        /// <summary>
        /// Confirms that a login attempt without a password
        /// fails the login model's validation rules.
        /// </summary>
        [Fact]
        public void LoginWithoutPassword_IsRejected()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Username = "SafeVaultUser",
                Password = string.Empty
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.NotEmpty(results);

            Assert.Contains(
                results,
                result =>
                    result.MemberNames.Contains(
                        nameof(LoginViewModel.Password)));
        }
    }
}