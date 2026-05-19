using System;
using System.Security.Cryptography;
using System.Text;
using SmartEngineeringWorkspaceManager.Models;
using System.Data.SQLite;

namespace SmartEngineeringWorkspaceManager.Services
{
    public class AuthenticationService
    {
        private readonly DatabaseService _databaseService;

        public AuthenticationService()
        {
            _databaseService = new DatabaseService();
        }

        // Current authenticated user.
        public static User CurrentUser { get; private set; }

        public bool Login(string username, string password)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string query = "SELECT UserId, Username, Password, Role, CreatedAt FROM Users WHERE Username = @Username";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPasswordHash = reader["Password"].ToString();
                            if (VerifyPassword(password, storedPasswordHash))
                            {
                                CurrentUser = new User
                                {
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Username = reader["Username"].ToString(),
                                    Role = reader["Role"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                                };
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public bool RegisterUser(string username, string password, string role)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    string query = "INSERT INTO Users (Username, Password, Role) VALUES (@Username, @Password, @Role)";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", HashPassword(password));
                        command.Parameters.AddWithValue("@Role", role);

                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch(SQLiteException)
            {
               // Typically happens when the Unique constraint on username fails.
               return false;
            }
        }

        // Basic hashing of passwords.
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private bool VerifyPassword(string inputPassword, string storedPasswordHash)
        {
            string hashOfInput = HashPassword(inputPassword);
            return hashOfInput == storedPasswordHash;
        }
    }
}