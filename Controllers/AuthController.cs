using Microsoft.AspNetCore.Mvc;
using MAdvice.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using MAdvice.ParameterModels;

namespace MAdvice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync([FromBody]LoginModel model)
        {
            var user = await _authService.LoginAsync(model.Username, model.Password.ToSha1());
            if (user == null)
                return BadRequest(new { message = "Kullanıcı adı veya şifresi hatalı." });

            return Ok(user);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync([FromBody]RegisterModel model)
        {
            var user = await _authService.RegisterAsync(model.Username, model.Password,model.Email);
            if (user == null)
                return BadRequest(new { message = "Kullanıcı adı kullanılıyor." });

            return Ok(user);
        }
    }
}