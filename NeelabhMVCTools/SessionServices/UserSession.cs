using Microsoft.AspNetCore.Http;
using NeelabhMVCTools.CipherServices;

namespace NeelabhMVCTools.SessionServices
{
    public class UserSession
    {
        private readonly ICipherService _cipherService;

        public UserSession(ICipherService cipherService)
        {
            _cipherService = cipherService;
        }

        public void Session(HttpContext httpContext, string key, string value)
        {
            if (value == null) httpContext.Session.Remove(key);
            else httpContext.Session.SetString(key, _cipherService.Encrypt(value));
        }

        public string Session(HttpContext httpContext, string key)
        {
            string value = httpContext.Session.GetString(key);
            if (value == null) return null;
            return _cipherService.Decrypt(value);
        }
    }
}