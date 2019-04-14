using Microsoft.AspNetCore.Identity;

namespace IdentityMicroservice
{
    public class ApplicationUser : IdentityUser
    {
        public string Role { get; set; }
    }
}
