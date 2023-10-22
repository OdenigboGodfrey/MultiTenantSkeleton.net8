using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace webapi_80.src.Shared.Utilities
{
    public static class JwtokenOptions
    {

        public const string Issuer = "http://localhost:8753/";

        public const string Audience = "http://localhost:4200/";

        public const string Key = "supersecret_!@@#!!!!=--=-90-556872%$#$#$%@$^&%^[][gg00gvtftf1!!@@%^^^secretkey!12345";
        public const int JwtExpireDays = 30;

        public static SecurityKey GetSecurityKey() =>
         new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));

        }

}


