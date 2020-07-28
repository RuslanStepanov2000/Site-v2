using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Tatneft.Data
{
     public class User
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        public string Salt { get; set; }
        public string Id { get; set; }
        public ClaimsIdentity ClaimsIdentity { get; set; }
        public ClaimsPrincipal ClaimsPrincipal { get; set; }
    }
}
