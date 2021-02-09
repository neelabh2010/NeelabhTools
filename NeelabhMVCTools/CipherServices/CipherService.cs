using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace NeelabhMVCTools.CipherServices
{
    public class CipherService : ICipherService
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public string EncryptionKey;
        public int UserKeyTimeOut;

        public CipherService(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtectionProvider = dataProtectionProvider;
            SetSettings();
        }

        private void SetSettings()
        {
            try
            {
                IConfigurationBuilder builder = new ConfigurationBuilder();
                builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
                var root = builder.Build();

                EncryptionKey = root.GetSection("NT_CipherSettings").GetSection("EncryptionKey").Value;
                UserKeyTimeOut = root.GetSection("NT_CipherSettings").GetSection("UserKeyTimeout").Value.ToInt();
            }
            catch
            {
                EncryptionKey = string.Empty;
                UserKeyTimeOut = 0;
            }
        }

        public string Encrypt(string input, string userTransKey = "")
        {
            string key = EncryptionKey + userTransKey;
            var protector = _dataProtectionProvider.CreateProtector(key);
            return protector.Protect(input);
        }

        public string Decrypt(string cipherText, string userTransEncKey = "")
        {
            string userTransKey = "";

            if (userTransEncKey != "")
            {
                userTransKey = Decrypt(userTransEncKey);
                bool isSuccess = DateTime.TryParse(userTransKey, out DateTime key_dt);

                if (!isSuccess)
                    throw new CipherException("Invalid user key provided.");
                else if (DateTools.CurrentDateTime() > key_dt.AddMinutes(UserKeyTimeOut))
                    throw new CipherException("User key has been expired. Reload the page and try again.");
            }

            try
            {
                string key = EncryptionKey + userTransKey;
                var protector = _dataProtectionProvider.CreateProtector(key);
                return protector.Unprotect(cipherText);
            }
            catch
            {
                throw new CipherException("Unable to resolve the cipher service.");
            }
        }

        public string UserTransKey()
        {
            return DateTools.GetDateTime("dd-MMM-yyyy HH:mm:ss:fff");
        }

    }
}
