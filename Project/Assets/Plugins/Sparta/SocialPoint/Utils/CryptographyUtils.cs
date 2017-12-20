using System.Security.Cryptography;
using System.Text;
using System;

namespace SocialPoint.Utils
{
    public sealed class CryptographyUtils
    {
        public static string GetHashSha256(string original)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(original);
            SHA256Managed sha = new SHA256Managed();
            byte[] hash = sha.ComputeHash(bytes);
            int hashSize = (2 * hash.Length) + (int)Math.Ceiling((double)hash.Length / 4.0) - 1;//Capacity = 2 chars for each byte (hexadecimal) + dashes every 4 bytes
            StringBuilder hashString = new StringBuilder(hashSize);
            for(int i = 0; i < hash.Length; ++i)
            {
                // Add a dash every four bytes, for readability.
                if(i != 0 && i % 4 == 0)
                {
                    hashString.Append('-');
                }
                hashString.AppendFormat("{0:x2}", hash[i]);
            }
            return hashString.ToString();
        }

        public static int HashCombine(int combined, int hash)
        {
            // This is based on boost::hash_combine
            return combined ^ (hash + 486187739 + (combined << 6) + (combined >> 2));
        }
    }
}