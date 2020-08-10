using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Formatting;
using Microsoft.AspNetCore.Mvc;
using Tatneft.Data;
using Tatneft.Servises;

namespace Tatneft.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        /// <summary>
        /// Аутентфиикация и получение токена
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns>Token</returns>
        [HttpPost]
        [Route("/api/[controller]/PostTokenGet")]
        public IActionResult PostTokenGet([FromBody] User userModel)
        {
            userModel = new UserService().UserGet(userModel.Email, userModel.Password);
            if (userModel.Email != null)
            {
                return Ok(userModel.Token);
            }
            else return BadRequest();
        }
        /// <summary>
        /// Удлаение токена в конце сессии
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns>Void</returns>
        [HttpPost]
        [Route("/api/[controller]/PostTokenClean")]
        public IActionResult PostTokenClean([FromBody] User userModel)
        {
            try
            {
                new UserService().UserTokenSet(userModel, "");
                return Ok();

            }
            catch
            {
                return BadRequest();
            }
        } 
    }

}