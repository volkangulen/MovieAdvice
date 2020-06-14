using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace System
{
    public static class Extensions
    {
        public static string ToSha1(this string value)
        {
            return HashOf<SHA1CryptoServiceProvider>(value, Encoding.Default);
        }

        private static string HashOf<T>(string text, Encoding enc)
            where T : HashAlgorithm, new()
        {
            var buffer = enc.GetBytes(text);
            var provider = new T();
            return BitConverter.ToString(provider.ComputeHash(buffer)).Replace("-","");
        }
        public static int GetUserId(this HttpContext httpContext)
        {
            var userData = httpContext.User.Claims.SingleOrDefault(q => q.Type == ClaimTypes.UserData);
            return userData == null ? 0 : int.Parse(userData.Value);
        }
        public static string GetUserEmail(this HttpContext httpContext)
        {
            var userData = httpContext.User.Claims.SingleOrDefault(q => q.Type == ClaimTypes.Email);
            return userData?.Value;
        }
        public static string GetUsername(this HttpContext httpContext)
        {
            var userData = httpContext.User.Claims.SingleOrDefault(q => q.Type == ClaimTypes.Name);
            return userData?.Value;
        }
    }
   

}
