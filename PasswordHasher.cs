using Microsoft.AspNetCore.Identity;

namespace Spaceship;

public class PasswordHasher
{

    public string HashPassword(string password)
    {
        string salt = BCrypt.Net.BCrypt.GenerateSalt(12);

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

        return hashedPassword;
    }
}