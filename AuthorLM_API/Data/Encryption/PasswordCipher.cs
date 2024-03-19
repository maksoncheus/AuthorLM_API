using System.Security.Cryptography;
using System.Text;

namespace AuthorLM_API.Data.Encryption
{
    public static class PasswordCipher
    {
        // Данная константа нужна для определения размера ключа алгоритма шифрования в битах.
        // Позже происходит деление на 8 для получения соответствующего количества байт.
        private const int _keysize = 128;

        // Данная константа определяет количество итераций метода генерации байт пароля.
        private const int _derivationIterations = 1000;
        private const string passPhrase = "AuthorLMSuperSecureStringNeverBeenHacked";
        public static string Encrypt(string plainText)
        {
            // Соль и исходный вектор генерируются каждый вызов метода случайным образом,
            // но они добавлены в начало зашифрованного пароля,
            // так что такие же соль и ИВ могут быть использованы во время дешифровки.  
            var saltStringBytes = Generate128BitsOfRandomEntropy();
            var ivStringBytes = Generate128BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, _derivationIterations))
            {
                var keyBytes = password.GetBytes(_keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            // Получить полный поток байтов, представляющих:
            // [32 байта соли] + [32 ИВ] + [n байт зашифрованного текста]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Первые 32 байта - соль.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(_keysize / 8).ToArray();
            // Следующие 32 байта - ИВ.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(_keysize / 8).Take(_keysize / 8).ToArray();
            // Зашифрованный текст без соли и ИВ.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((_keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((_keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, _derivationIterations))
            {
                var keyBytes = password.GetBytes(_keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            using (var streamReader = new StreamReader(cryptoStream, Encoding.UTF8))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate128BitsOfRandomEntropy()
        {
            var randomBytes = new byte[16];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
