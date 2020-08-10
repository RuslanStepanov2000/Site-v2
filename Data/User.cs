using Microsoft.AspNetCore.Identity;

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
        //public IdentityRole Role { get; set; }
    }

}
