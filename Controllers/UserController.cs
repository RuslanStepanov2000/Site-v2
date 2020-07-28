using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tatneft.Data;
using Tatneft.Servises;

namespace Tatneft.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        [HttpPost]
        public IActionResult Response1()
        {
            if (email == "ruslan@mail.ru")
            {
                return Ok(email);
            }
            else return BadRequest();
        }
            
    }
}