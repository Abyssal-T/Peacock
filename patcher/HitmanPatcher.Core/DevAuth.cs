using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace HitmanPatcher
{
    /// <summary>
    /// Password protection for the Dev Panel. The password never leaves this machine and is not stored:
    /// it is stretched into a key, and only a SHA-256 hash of that key is embedded here (and in
    /// components/devEntitlements.ts). The server only honours a devEntitlements.txt that is signed
    /// with the key, so the file cannot be written by hand and expires on its own.
    /// </summary>
    public static class DevAuth
    {
        private const string Salt = "peacock-dev-entitlements-v1";
        private const int Iterations = 200000;
        private const string KeyVerifier = "0f2eb7d692c2e4a0ec0b7d0e4cccefe7d5cbed3929ae56cd13d3f56f34fdbd0f";

        public static readonly TimeSpan FileLifetime = TimeSpan.FromHours(24);

        /// <summary>
        /// Returns the hex dev key if the password is right, otherwise null.
        /// </summary>
        public static string TryDeriveKey(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return null;
            }

            byte[] key;

            using (Rfc2898DeriveBytes kdf = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(Salt), Iterations))
            {
                key = kdf.GetBytes(32);
            }

            string hash;

            using (SHA256 sha = SHA256.Create())
            {
                hash = ToHex(sha.ComputeHash(key));
            }

            return string.Equals(hash, KeyVerifier, StringComparison.Ordinal) ? ToHex(key) : null;
        }

        /// <summary>
        /// Builds the lines of a signed devEntitlements.txt.
        /// </summary>
        public static string[] BuildFileLines(string keyHex, IEnumerable<string> products, DateTime utcNow)
        {
            List<string> list = new List<string>(products);
            long expires = (long)(utcNow + FileLifetime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            string payload = expires + "\n" + string.Join("\n", list);

            string sig;

            using (HMACSHA256 hmac = new HMACSHA256(FromHex(keyHex)))
            {
                sig = ToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
            }

            List<string> lines = new List<string>
            {
                "#auth:" + keyHex,
                "#expires:" + expires,
                "#sig:" + sig,
            };
            lines.AddRange(list);
            return lines.ToArray();
        }

        private static string ToHex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);

            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        private static byte[] FromHex(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }
    }
}
