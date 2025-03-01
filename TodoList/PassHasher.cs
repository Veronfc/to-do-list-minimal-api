using System.Security.Cryptography;
using System.Text;

public static class PassHasher
{
  public static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
  {
    using (var hmac = new HMACSHA512())
    {
      salt = hmac.Key;

      byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

      hash = hmac.ComputeHash(passwordBytes);
    }
  }

  public static bool VerifyPasswordHash(string password, byte[] hash, byte[] salt)
  {
    using (var hmac = new HMACSHA512(salt))
    {
      byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

      byte[] computedHash = hmac.ComputeHash(passwordBytes);

      bool match = computedHash.SequenceEqual(hash);

      return match;
    }
  }
}