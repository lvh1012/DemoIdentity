using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoIdentity.Controllers
{
    [Authorize(Policy = "AtLeast21")]
    public class PolicyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
