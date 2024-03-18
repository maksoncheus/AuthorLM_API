using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthorLM_API.Models.Auth
{
    public class AuthOptions
    {
        public const string ISSUER = "AuthorLM"; // издатель токена
        public const string AUDIENCE = "AuthorLM_App_Client"; // потребитель токена
        const string KEY = "CipherKeyAuthorLMSuperSecureMegaSecretKEYKEY123";   // ключ для шифрации
        public const int LIFETIME = 525600; // время жизни токена - 1 минута
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
