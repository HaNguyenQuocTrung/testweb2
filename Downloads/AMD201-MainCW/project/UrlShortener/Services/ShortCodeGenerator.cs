using System.Security.Cryptography;
using System.Text;

namespace UrlShortener.Services
{
    // Generates URL-safe Base62 codes (0-9, a-z, A-Z)
    public class ShortCodeGenerator : IShortCodeGenerator
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public string Generate(int length = 8)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                // Map byte to 0..61
                var idx = bytes[i] % Alphabet.Length;
                sb.Append(Alphabet[(int)idx]);
            }

            return sb.ToString();
        }
    }
}
