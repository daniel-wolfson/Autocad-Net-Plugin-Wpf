using ID.Infrastructure.Core;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace General.Infrastructure.Helpers
{
    public class Util1
    {
        private static readonly object _encryptLocker = new object();
        private static readonly object _decryptLocker = new object();
        private static string TEXT_ENCRYPT_DECRYPT_KEY = "crpm2019castcrpm";
        private static string PSW_ENCRYPT_DECRYPT_KEY = "19541954";

        //private static string _AUTH_KEY = "0A652F06-A920-493B-A447-B3D463B0D84C";
        public static string AUTH_KEY => AuthOptions.KEY;

        static Util1()
        {
            // TODO: read config
            TEXT_ENCRYPT_DECRYPT_KEY = "crpm2019castcrpm"; //AppConfig.EncryptDecryptKey;
        }

        private static Lazy<IAppConfig> _appConfig = new Lazy<IAppConfig>(() =>
            {
                IAppConfig appConfig = GeneralContext.GetService<IAppConfig>();
                return appConfig;
            });
        public static IAuthOptions AuthOptions => _authOptions.Value;
        private static Lazy<IAuthOptions> _authOptions = new Lazy<IAuthOptions>(() =>
        {
            IAuthOptions authOptions = GeneralContext.GetService<IAuthOptions>();
            return authOptions;
        });
        public static IAppConfig AppConfig => _appConfig.Value;

        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        // Convert a byte array to an Object
        public static object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            object obj = binForm.Deserialize(memStream);
            return obj;
        }

        public static string CreateGuid()
        {
            Guid g = Guid.NewGuid();
            string guid = g.ToString();

            return guid.Replace("-", "");
        }

        public static string ConvertDateToString(DateTime CurrDate)
        {
            string retval = CurrDate.ToString("yyyyMMddHHmmss");

            return retval;
        }

        public static DateTime ConvertStringToDate(string strDate)
        {
            DateTime retDate;

            int Year;
            int Month;
            int Day;
            int Hour;
            int Minute;
            int Second;

            try
            {
                Year = int.Parse(strDate.Substring(0, 4));
                Month = int.Parse(strDate.Substring(4, 2));
                Day = int.Parse(strDate.Substring(6, 2));
                Hour = int.Parse(strDate.Substring(8, 2));
                Minute = int.Parse(strDate.Substring(10, 2));
                Second = int.Parse(strDate.Substring(12, 2));

                retDate = new DateTime(Year, Month, Day, Hour, Minute, Second);

                return retDate;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #region Log Message

        public static void WriteErrorLogging(string strPath, string strLog)
        {
            // Create a file for output named TestFile.txt.
            using (FileStream myFileStream =
                new FileStream(strPath, FileMode.Append))
            {
                // Create a new text writer using the output stream 
                // and add it to the trace listeners.
                TextWriterTraceListener myTextListener =
                    new TextWriterTraceListener(myFileStream);
                Trace.Listeners.Add(myTextListener);

                // Write output to the file.
                Trace.WriteLine(strLog);

                // Flush and close the output stream.
                Trace.Flush();
                Trace.Close();
            }
        }
        public static void LogMessage(string source, string message, TraceEventType severity, params object[] parameters)
        {
            string Message = string.Format("\nSource: {0}\nMessage: {1}\nParameters:{2} ", source, message, ConvertParametersToMessage(parameters));
        }

        private static string ConvertParametersToMessage(params object[] parameters)
        {
            string msg = string.Empty;
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    IEnumerable collection = parameters[0] as IEnumerable;
                    if (collection != null)
                    {
                        foreach (var item in collection)
                        {
                            msg += item.ToString() + " ";
                        }
                    }
                    else
                    {
                        msg += parameters[0].ToString() + " ";
                    }
                }
            }
            return msg;
        }
        #endregion

        public static string ReplaceLast(string find, string replace, string str)
        {
            int lastIndex = str.LastIndexOf(find);

            if (lastIndex == -1)
            {
                return str;
            }

            string beginString = str.Substring(0, lastIndex);
            string endString = str.Substring(lastIndex + find.Length);

            return beginString + replace + endString;
        }

        #region Encrypt / Decrypt

        /// <summary> Encrypt password by Rijndael symmetric algorithm</summary>
        public static string EncryptPassword(string text, string key = null)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(text);
            byte[] salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

            using (PasswordDeriveBytes pdb = new PasswordDeriveBytes(key ?? PSW_ENCRYPT_DECRYPT_KEY, salt))
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
        public static string DecryptPassword(string text, string key = null)
        {
            byte[] buffer = Convert.FromBase64String(text);
            byte[] salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

            using (PasswordDeriveBytes pdb = new PasswordDeriveBytes(key ?? PSW_ENCRYPT_DECRYPT_KEY, salt))
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
        public static string EncryptData<T>(T data)
        {
            string plainText = JsonConvert.SerializeObject(data);
            var encryptResult = EncryptText(plainText);
            return encryptResult;
        }

        /// <summary> Encrypt text by Aes symmetric algorithm</summary>
        public static string EncryptText(string plainText)
        {
            // Check arguments.
            byte[] encrypted;

            // Create an Aes object with the specified key and IV.
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(TEXT_ENCRYPT_DECRYPT_KEY);
                aes.IV = Encoding.UTF8.GetBytes(TEXT_ENCRYPT_DECRYPT_KEY);
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
        public static T DecryptData<T>(string encryptedData)
        {
            var dataString = DecryptText(encryptedData);
            T data = JsonConvert.DeserializeObject<T>(dataString);
            return data;
        }

        /// <summary> Decrypt text by AES symmetric algorithm</summary>
        public static string DecryptText(string encryptedText)
        {
            // Declare the string used to hold the decrypted text.
            string plaintext = null;
            byte[] cipherText = Convert.FromBase64String(encryptedText);

            // Create an Aes object with the specified key and IV.
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(TEXT_ENCRYPT_DECRYPT_KEY);
                aes.IV = Encoding.UTF8.GetBytes(TEXT_ENCRYPT_DECRYPT_KEY);
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

        #endregion Encrypt / Decrypt

        #region Token

        /// <summary> read token by objectType and claimType /// </summary>
        public static T ReadToken<T>(string token, string claimType = null)
        {
            var isTokenValid = VerifyToken(token);
            if (!isTokenValid)
                return default;

            var handler = new JwtSecurityTokenHandler();
            //string authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            //token = token.Replace("Bearer ", "");
            //var jsonToken = handler.ReadToken(token);
            //var payload = token.Split(".")[1];
            //var t = handler.ReadJwtToken(token);
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (claimType == null && typeof(T) == typeof(IAppUser))
            {
                claimType = ClaimTypes.UserData;
            }

            string value = string.Empty;
            if (typeof(T) == typeof(IAppUser))
            {
                value = jwtToken.Claims.First(claim => claim.Type == claimType).Value;
                value = DecryptText(value);
            }
            else if (typeof(T) == typeof(List<Claim>))
            {
                return (T)Convert.ChangeType(jwtToken.Claims, typeof(T));
            }

            T data = JsonConvert.DeserializeObject<T>(value);
            return data;
        }

        /// <summary> create token /// </summary>
        public static string CreateToken(IAppUser user, List<Claim> customClaims = null)
        {
            //var _authOptions = GeneralContext.GetService<AuthOptions>();
            IAuthOptions authOptions = GeneralContext.GetService<IAuthOptions>();
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authOptions.KEY));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var userData = user.ToStringJson();

            var encryptedUserData = EncryptText(userData);
            var sss = DecryptText(encryptedUserData);
            var identityClaims = new List<Claim>() { new Claim(ClaimTypes.UserData, encryptedUserData) };

            customClaims?.ToList().ForEach(x =>
                identityClaims.Add(new Claim(x.Type, x.Value))
            );

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(identityClaims),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.Now.AddMinutes(20),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenObj = tokenHandler.CreateToken(tokenDescriptor);
            var tokenResult = tokenHandler.WriteToken(tokenObj);

            return tokenResult;
        }

        /// <summary> verify token by TokenValidationParameters /// </summary>
        public static bool VerifyToken(string token)
        {
            var key = Encoding.UTF8.GetBytes(AUTH_KEY);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken = null;
            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch (SecurityTokenException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Log.Logger.WarningEx(ex); //something else happened
            }
            //... manual validations return false if anything untoward is discovered
            return validatedToken != null;
        }

        #endregion

        #region Cookies

        /// <summary> set the cookie, expireTime(min) /// </summary>  
        public static void SetCookie(string key, string value, int? expireTime = null)
        {
            CookieOptions option = new CookieOptions();

            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddHours(24);

            IHttpContextAccessor httpContextAccessor = GeneralContext.GetService<IHttpContextAccessor>();
            //var cookie = new Cookie(CookiesKeys.TokenData, value);

            httpContextAccessor.HttpContext.Response.Cookies.Append(CookiesKeys.TokenData, value);
        }

        /// <summary> set the cookie /// </summary>  
        public static string GetCookie(string key)
        {
            IHttpContextAccessor httpContextAccessor = GeneralContext.GetService<IHttpContextAccessor>();
            string value = httpContextAccessor.HttpContext.Request.Cookies[key];
            return value;
        }

        /// <summary> Delete the key /// </summary>  
        public static void RemoveCookie(string key)
        {
            IHttpContextAccessor httpContextAccessor = GeneralContext.GetService<IHttpContextAccessor>();
            httpContextAccessor.HttpContext.Response.Cookies.Delete(key);
        }

        #endregion Cookies
    }
}


