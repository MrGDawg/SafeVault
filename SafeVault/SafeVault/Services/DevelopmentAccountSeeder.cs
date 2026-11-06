using SafeVault.Data;
using SafeVault.Models;

namespace SafeVault.Services
{
    /// <summary>
    /// Creates controlled authentication accounts for local development.
    /// Plaintext passwords are supplied through configuration and are
    /// hashed before account information is written to the database.
    /// </summary>
    public class DevelopmentAccountSeeder
    {
        private readonly SafeVaultDatabase _database;
        private readonly PasswordService _passwordService;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes the seeder with its database, password-hashing,
        /// and configuration dependencies.
        /// </summary>
        public DevelopmentAccountSeeder(
            SafeVaultDatabase database,
            PasswordService passwordService,
            IConfiguration configuration)
        {
            _database = database;
            _passwordService = passwordService;
            _configuration = configuration;
        }

        /// <summary>
        /// Creates the controlled development accounts when their
        /// configuration settings exist and the accounts have not
        /// already been created.
        /// </summary>
        public void Seed()
        {
            SeedAccount("DevelopmentAccount");
            SeedAccount("DevelopmentUserAccount");
        }

        /// <summary>
        /// Creates one development account from the specified
        /// configuration section.
        /// </summary>
        private void SeedAccount(string configurationSection)
        {
            var username =
                _configuration[$"{configurationSection}:Username"];

            var email =
                _configuration[$"{configurationSection}:Email"];

            var password =
                _configuration[$"{configurationSection}:Password"];

            var role =
                _configuration[$"{configurationSection}:Role"];

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(role))
            {
                return;
            }

            if (_database.UserAccountExists(username))
            {
                return;
            }

            var user = new UserAccount
            {
                Username = username,
                Email = email,
                Role = role
            };

            // Only the generated password hash is stored in the database.
            user.PasswordHash =
                _passwordService.HashPassword(
                    user,
                    password);

            _database.AddUserAccount(user);
        }
    }
}