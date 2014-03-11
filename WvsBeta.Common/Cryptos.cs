using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security.Cryptography;

namespace WvsBeta.Common
{
    public class Cryptos
    {
        public static string GetMD5Hash(string input)
        {
            MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }

        public static string SHA1_ComputeHexaHash(string text)
        {
            // Gets the SHA1 hash for text

            SHA1Managed SHhash = new SHA1Managed();
            byte[] data = Encoding.ASCII.GetBytes(text);
            byte[] hash = SHhash.ComputeHash(data);
            // Transforms as hexa
            string hexaHash = "";
            foreach (byte b in hash)
            {
                hexaHash += String.Format("{0:x2}", b);
            }
            // Returns SHA1 hexa hash
            return hexaHash;
        }

        public static string SHA512_ComputeHexaHash(string text)
        {
            // Gets the SHA512 hash for text

            SHA512Managed SHhash = new SHA512Managed();
            byte[] data = Encoding.ASCII.GetBytes(text);
            byte[] hash = SHhash.ComputeHash(data);
            // Transforms as hexa
            string hexaHash = "";
            foreach (byte b in hash)
            {
                hexaHash += String.Format("{0:x2}", b);
            }
            // Returns SHA1 hexa hash
            return hexaHash;
        }

        static Random rnd = new Random();
        public static string GetNewSessionHash()
        {
            string wat = rnd.Next(0, 1000) + "-" + rnd.Next(1000, 3000) + "-" + rnd.Next(0, 90090); // random?? :D
            return SHA512_ComputeHexaHash(wat);
        }

    }
}
