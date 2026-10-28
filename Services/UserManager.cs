
using AuthDemo.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace AuthDemo.Services
{
    public class UserManager
    {
        private readonly string _filePath;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UserManager(IWebHostEnvironment environment)
        {
            _filePath = Path.Combine(
                environment.ContentRootPath,
                "data",
                "users.json"
            );
        }

        public List<User> LoadUsers()
        {
            if (!File.Exists(_filePath))
                return new List<User>();

            string json = File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize<List<User>>(json)
                ?? new List<User>();
        }

        public bool ValidateCredentials(string username, string password)
        {
            User? user = LoadUsers().FirstOrDefault(
                u => u.Username == username
            );

            if (user == null)
                return false;

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password
            );

            return result != PasswordVerificationResult.Failed;
        }

        public void SeedAdmin()
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName(_filePath)!
            );

            if (File.Exists(_filePath))
                return;

            User admin = new User
            {
                Username = "admin"
            };

            admin.PasswordHash = _passwordHasher.HashPassword(
                admin,
                "Password123!"
            );

            List<User> users = new List<User> { admin };

            string json = JsonSerializer.Serialize(
                users,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }
            );

            File.WriteAllText(_filePath, json);
        }
    }
}
