using System;
using System.Text;

namespace SSS.CommonLib
{
    public static class CipherHelper
    {
        public static string Encrypt(string plainText)
        {
            if (plainText == null)
            {
                return null;
            }

            var bytesToBeEncrypted = Encoding.UTF8.GetBytes(plainText);

            return Convert.ToBase64String(bytesToBeEncrypted);
        }

        public static string Decrypt(string encryptedText)
        {
            try
            {
                if (encryptedText == null)
                {
                    return null;
                }

                var bytesToBeDecrypted = Convert.FromBase64String(encryptedText);

                return Encoding.UTF8.GetString(bytesToBeDecrypted);
            }
            catch
            {
                return string.Empty;
            }
        }

    }
}
