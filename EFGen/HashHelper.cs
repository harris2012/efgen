using System;
using System.Collections.Generic;
using System.Text;

using System.Security.Cryptography;

namespace EFGen.Helper
{
    public static class HashHelper
    {
        public static string ComputeHash(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            using (var provider = SHA256.Create())
            {
                var hash = provider.ComputeHash(bytes);

                return Convert.ToBase64String(hash);
            }
        }
    }
}
