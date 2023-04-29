using Microsoft.AspNetCore.Identity;

namespace DemoIdentity.Areas.Identity.Data
{
    // custom user mặc định của thư viện
    public class CustomUser : IdentityUser
    {
        [PersonalData]
        public string? Name { get; set; }

        [PersonalData]
        public DateTime DOB { get; set; }
    }
}