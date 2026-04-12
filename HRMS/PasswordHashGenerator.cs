// Utility class to generate password hashes for creating users
// This is for development/testing purposes only

using System.Security.Cryptography;

namespace HRMS.Utilities;

public class PasswordHashGenerator
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    // Example usage in a console app or unit test:
    public static void Main(string[] args)
    {
        Console.WriteLine("Password Hash Generator");
        Console.WriteLine("======================");
        Console.WriteLine();

        if (args.Length > 0)
        {
            foreach (var password in args)
            {
                var hash = HashPassword(password);
                Console.WriteLine($"Password: {password}");
                Console.WriteLine($"Hash: {hash}");
                Console.WriteLine();
            }
        }
        else
        {
            Console.Write("Enter password to hash: ");
            var password = Console.ReadLine();

            if (!string.IsNullOrEmpty(password))
            {
                var hash = HashPassword(password);
                Console.WriteLine();
                Console.WriteLine($"Password Hash:");
                Console.WriteLine(hash);
                Console.WriteLine();
                Console.WriteLine("SQL Insert Example:");
                Console.WriteLine($"INSERT INTO Users (Username, EmailId, PasswordHash, IsActive, CreatedOn, CreatedBy)");
                Console.WriteLine($"VALUES ('username', 'email@domain.com', '{hash}', 1, GETDATE(), 'system');");
            }
        }
    }

    /*
     * To use this generator:
     *
     * Option 1: Run from command line with arguments
     * ------------------------------------------------
     * dotnet run --project . -- "MyPassword123"
     *
     * Option 2: Run interactively
     * -----------------------------
     * dotnet run --project .
     * Then enter the password when prompted
     *
     * Option 3: Use in your code
     * ---------------------------
     * var hash = PasswordHashGenerator.HashPassword("MyPassword123");
     *
     * Then use the hash in your SQL insert statement
     */
}
