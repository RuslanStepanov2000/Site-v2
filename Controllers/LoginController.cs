using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Tatneft.Data;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Tatneft.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string userId, string password)
        {
            User user = new User();
            user.Login = userId;
            user.Password = password;

            IActionResult response = Unauthorized();

            //user = AuthenticateUser(user);

            //Поиск пользователя в базе, добавление его роли
            user = new DBWorking().Auth(user);

            if (user.Login != null)
            {
                var tokenString = GenerateJSONWebToken(user);
                response = Ok(new { token = tokenString });
            }
            return response;

        }
        //private User AuthenticateUser(User login)
        //{
        //    User user = new User();
        //    if (login.login == "ruslan2" && login.password == "Ruslan2411")
        //    {
        //        user = new User { login = "ruslan2", password = "Ruslan2411", role = "Admin", email="ssss@mail.ru" };
        //    }
        //    return user;
        //}

        private string GenerateJSONWebToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credintalis = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            if (user.Login != null)
            {
                var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.Login),
            //new Claim(JwtRegisteredClaimNames.Email, user.email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role)
                 };



                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Issuer"],
                    claims,
                    expires: DateTime.Now.AddHours(24),
                    signingCredentials: credintalis);
                var encodetoken = new JwtSecurityTokenHandler().WriteToken(token);
                return encodetoken;
            }
            else {
                return "не дам токен, не правильный логин/пароль";
            }
            

        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Post([FromBody] string value)
        {
            try
            {
                return Ok(value);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
