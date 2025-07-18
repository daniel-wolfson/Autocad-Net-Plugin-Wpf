using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ID.Infrastructure.Helpers
{
    /// <summary> Encrypt / decrypt </summary>
    public partial class Util
    {
        public static string Encrypt(string EncryptText, string key)
        {

            byte[] clearBytes = Encoding.Unicode.GetBytes(EncryptText);

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(key,
                new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,
                            0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});

            byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));

            return Convert.ToBase64String(encryptedData);

        }

        public static string Decrypt(string CipherText, string key)
        {
            byte[] cipherBytes = Convert.FromBase64String(CipherText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(key,
                new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65,
                            0x64, 0x76, 0x65, 0x64, 0x65, 0x76});

            byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));

            return Encoding.Unicode.GetString(decryptedData);
        }

        private static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            // Create a MemoryStream to accept the encrypted bytes 
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearData, 0, clearData.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();

            return encryptedData;
        }

        private static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms,
            alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();

            return decryptedData;
        }

        /// <summary> Encrypt password by Rijndael symmetric algorithm</summary>
        public static string EncryptPassword(string text, string key)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(text);
            byte[] salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

            using (PasswordDeriveBytes pdb = new PasswordDeriveBytes(key, salt))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (Rijndael alg = Rijndael.Create())
                    {
                        alg.Key = pdb.GetBytes(32);
                        alg.IV = pdb.GetBytes(16);
                        CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
                        cs.Write(buffer, 0, buffer.Length);
                        cs.Close();
                        byte[] encryptedData = ms.ToArray();

                        return Convert.ToBase64String(encryptedData);
                    }
                }
            }
        }

        /// <summary> Decrypt password by Rijndael symmetric algorithm</summary>
        public static string DecryptPassword(string text, string key)
        {
            byte[] buffer = Convert.FromBase64String(text);
            byte[] salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

            using (PasswordDeriveBytes pdb = new PasswordDeriveBytes(key, salt))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (Rijndael alg = Rijndael.Create())
                    {
                        alg.Key = pdb.GetBytes(32);
                        alg.IV = pdb.GetBytes(16);
                        CryptoStream cs = new CryptoStream(ms,
                        alg.CreateDecryptor(), CryptoStreamMode.Write);
                        cs.Write(buffer, 0, buffer.Length);
                        cs.Close();
                        byte[] decryptedData = ms.ToArray();
                        return Encoding.Unicode.GetString(decryptedData);
                    }
                }
            }
        }

        /// <summary> Encrypt data by Aes symmetric algorithm</summary>
        public static string EncryptData<T>(T data, string key)
        {
            string plainText = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var encryptResult = EncryptText(plainText, key);
            return encryptResult;
        }

        /// <summary> Encrypt text by Aes symmetric algorithm</summary>
        public static string EncryptText(string plainText, string key)
        {
            // Check arguments.
            byte[] encrypted;

            // Create an Aes object with the specified key and IV.
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(key);
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return Convert.ToBase64String(encrypted);
        }

        /// <summary> Decrypt data by Aes symmetric algorithm</summary>
        public static T DecryptData<T>(string encryptedData, string key)
        {
            var dataString = DecryptText(encryptedData, key);
            T data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dataString);
            return data;
        }

        /// <summary> Decrypt text by AES symmetric algorithm</summary>
        public static string DecryptText(string encryptedText, string key)
        {
            // Declare the string used to hold the decrypted text.
            string plaintext = null;
            byte[] cipherText = Convert.FromBase64String(encryptedText);

            // Create an Aes object with the specified key and IV.
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(key);
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }

}
