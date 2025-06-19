using System.Text;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace WebApplication1.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly byte[] _key = Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef"); 
        private readonly byte[] _iv = Encoding.UTF8.GetBytes("abcdef9876543210"); 

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        public string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            using var decryptor = aes.CreateDecryptor();
            var cipherBytes = Convert.FromBase64String(cipherText);
            var decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }

}
