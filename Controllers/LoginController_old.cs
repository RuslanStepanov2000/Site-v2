//using System;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using Tatneft.Data;
//using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

//namespace Tatneft.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class LoginController_old : Controller
//    {
//        private IConfiguration _config;

//        public LoginController_old(IConfiguration config)
//        {
//            _config = config;
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        public IActionResult Login(string email, string password)
//        {
//            User user = new User();
//            user.Email = email;
//            user.Password = password;


//            IActionResult response;
//            //Поиск пользователя в базе
//            user = new DBWorkingSQLite().UserAuth(user);

//            if (user.Email != null)
//            {
//                //Создание и запись токена в бд
//                var tokenString = GenerateJSONWebToken(user);
//                new DBWorkingSQLite().UserTokenSet(user, tokenString);
//                response = Ok(new { token = tokenString });
//            }
//            else
//            {
//                response = Unauthorized();
//            }
//            return response;
//        }
//        //private User AuthenticateUser(User login)
//        //{
//        //    User user = new User();
//        //    if (login.login == "ruslan2" && login.password == "Ruslan2411")
//        //    {
//        //        user = new User { login = "ruslan2", password = "Ruslan2411", role = "Admin", email="ssss@mail.ru" };
//        //    }
//        //    return user;
//        //}

//        private string GenerateJSONWebToken(User user)
//        {
//            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
//            var credintalis = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

//            if (user.Email != null)
//            {
//                var claims = new[] {
//            new Claim(JwtRegisteredClaimNames.Email, user.Email),
//            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//            new Claim(ClaimTypes.Role, user.Role)
//                 };

//                var token = new JwtSecurityToken(
//                    issuer: _config["Jwt:Issuer"],
//                    audience: _config["Jwt:Issuer"],
//                    claims,
//                    expires: DateTime.Now.AddHours(24),
//                    signingCredentials: credintalis);
//                var encodetoken = new JwtSecurityTokenHandler().WriteToken(token);
//                return encodetoken;
//            }
//            else
//            {
//                return null;
//            }


//        }

//        [Authorize(Roles = "Admin")]
//        [HttpPost]
//        public IActionResult Post([FromBody] string value)
//        {
//            try
//            {
//                return Ok(value);
//            }
//            catch (Exception)
//            {
//                return NotFound();
//            }
//        }
//    }
//}
