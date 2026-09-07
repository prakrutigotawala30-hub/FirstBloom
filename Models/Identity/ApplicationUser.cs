using Microsoft.AspNetCore.Identity;

namespace FirstBloom.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}