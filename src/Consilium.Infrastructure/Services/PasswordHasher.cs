using System.Security.Cryptography;
using System.Text;
using Consilium.Application.Interfaces;

namespace Consilium.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        using (var rng = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA256))
        {
            byte[] salt = rng.Salt;
            byte[] hash = rng.GetBytes(HashSize);
            
            byte[] hashWithSalt = new byte[SaltSize + HashSize];
            Buffer.BlockCopy(salt, 0, hashWithSalt, 0, SaltSize);
            Buffer.BlockCopy(hash, 0, hashWithSalt, SaltSize, HashSize);
            
            return Convert.ToBase64String(hashWithSalt);
        }
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            byte[] hashWithSalt = Convert.FromBase64String(hash);
            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(hashWithSalt, 0, salt, 0, SaltSize);
            
            using (var rng = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] computedHash = rng.GetBytes(HashSize);
                
                for (int i = 0; i < HashSize; i++)
                {
                    if (hashWithSalt[SaltSize + i] != computedHash[i])
                        return false;
                }
                
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}
