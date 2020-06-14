using MAdvice.DAL;
using MAdvice.Extensions;
using MAdvice.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MAdvice.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly MovieContext _movieContext;

        public AuthService(IConfiguration configuration, MovieContext movieContext)
        {
            _movieContext = movieContext;
            _configuration = configuration;
        }

        

        /// <summary>
        /// Login için kullanılır.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Token bilgisi ile beraber <see cref="User"/> döner. </returns>
        public async Task<TaskResult<User>> LoginAsync(string username, string password)
        {
            try
            {
                //Kullanıcı adı ve şifresi ile kullanıcı sorgulanır. Birden fazla ya da herhangi bir kayıt gelmezse exception throwlar.
                var user = await _movieContext.Users.SingleAsync(q => q.Username == username && q.Password==password);

                GenerateJWT(ref user);

                return new TaskResult<User>(true,user);
            }
            //Exception alınırsa User null döner;
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Authentication için JWT üretir. 4 saat geçerlilik süresi bulunur.
        /// </summary>
        /// <param name="user"></param>
        private void GenerateJWT(ref User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                            //Claimlerle JWT Tokenından UserId, Username, Email verileri çıkartılabilir.
                            new Claim(ClaimTypes.UserData, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.Username.ToString()),
                            new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(4),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            //Token oluşturulur.
            var token = tokenHandler.CreateToken(tokenDescriptor);
            //Token veri tabanına kaydedilmeden geri döndürülür.
            user.Token = tokenHandler.WriteToken(token);
            user.TokenExpirationDate = tokenDescriptor.Expires.Value;
        }
        /// <summary>
        /// Yeni kullanıcı kayıt edilirken kullanılır. Kayıt işleminden sonra JWT üretilir.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <returns><see cref="User" /></returns>
        public async Task<TaskResult<User>> RegisterAsync(string username, string password,string email)
        {
            var user = await GetUser(username);
            if(user != null)
            {
                return new TaskResult<User>(false,null,"Aynı kullanıcı adına sahip farklı bir hesap bulunmakta.");
            }
            user = new User(username, password, email);
            await _movieContext.Users.AddAsync(user);
            await _movieContext.SaveChangesAsync();
            GenerateJWT(ref user);
            return new TaskResult<User>(true, user);

        }
        private async Task<User> GetUser(string username)
        {
            return await _movieContext.Users.FirstOrDefaultAsync(q => q.Username == username);
        }
    }
}
