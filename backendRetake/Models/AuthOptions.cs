using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace backendRetake.Models
{
    public class AuthOptions
    {
        public const string ISSUER = "HitsTeacher";
        public const string AUDIENCE = "HitsStudent";
        const string KEY = "IDontWantToSetTheWorldOnFire_2077";
        public const int LIFETIME = 60;
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
