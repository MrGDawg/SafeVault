using Microsoft.Data.Sqlite;
using SafeVault.Models;

namespace SafeVault.Data
{
    /// <summary>
    /// Provides database operations for the SafeVault application.
    /// User-supplied values are passed to SQL commands as parameters
    /// rather than being concatenated into SQL statements.
    /// </summary>
    public class SafeVaultDatabase
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes the database service and ensures that the
        /// required database structure exists.
        /// </summary>
        public SafeVaultDatabase()
        {
            _connectionString = "Data Source=safevault.db";
            InitializeDatabase();
        }

        /// <summary>
        /// Creates the Users table when it does not already exist.
        /// PasswordHash stores the protected password representation,
        /// while Role supports role-based authorization.
        /// </summary>
        private void InitializeDatabase()
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText =
                """
        CREATE TABLE IF NOT EXISTS Users
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Username TEXT NOT NULL,
            Email TEXT NOT NULL,
            PasswordHash TEXT,
            Role TEXT NOT NULL DEFAULT 'user'
        );
        """;

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Inserts validated user information submitted through the
        /// original Activity 1 form.
        /// </summary>
        public void AddUser(string username, string email)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText =
                """
                INSERT INTO Users (Username, Email)
                VALUES ($username, $email);
                """;

            // Parameters keep user-supplied values separate from SQL syntax.
            command.Parameters.AddWithValue(
                "$username",
                username);

            command.Parameters.AddWithValue(
                "$email",
                email);

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Inserts a complete authentication account into the database.
        /// The stored password value must already be hashed.
        /// </summary>
        public void AddUserAccount(UserAccount user)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText =
                """
                INSERT INTO Users
                    (Username, Email, PasswordHash, Role)
                VALUES
                    ($username, $email, $passwordHash, $role);
                """;

            // Every stored value remains separate from the SQL command text.
            command.Parameters.AddWithValue(
                "$username",
                user.Username);

            command.Parameters.AddWithValue(
                "$email",
                user.Email);

            command.Parameters.AddWithValue(
                "$passwordHash",
                user.PasswordHash);

            command.Parameters.AddWithValue(
                "$role",
                user.Role);

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Retrieves the first account matching the supplied username.
        /// Returns null when no matching account exists.
        /// </summary>
        public UserAccount? GetUserByUsername(string username)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText =
                """
                SELECT
                    Id,
                    Username,
                    Email,
                    PasswordHash,
                    Role
                FROM Users
                WHERE Username = $username
                  AND PasswordHash IS NOT NULL
                LIMIT 1;
                """;

            command.Parameters.AddWithValue(
                "$username",
                username);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return new UserAccount
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                Role = reader.GetString(4)
            };
        }

        /// <summary>
        /// Determines whether an authentication account already exists
        /// for the supplied username.
        /// </summary>
        public bool UserAccountExists(string username)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText =
                """
        SELECT COUNT(1)
        FROM Users
        WHERE Username = $username
          AND PasswordHash IS NOT NULL;
        """;

            command.Parameters.AddWithValue(
                "$username",
                username);

            var result = Convert.ToInt32(
                command.ExecuteScalar());

            return result > 0;
        }
    }
}