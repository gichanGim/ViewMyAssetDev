using Microsoft.AspNetCore.Identity;

namespace ViewMyAssetDev.Models
{
    public class Users : IdentityUser
    {
        public string UserId { get; set; }
    }
}
