using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoIdentity.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return Content("AllowAnonymous only");
        }

        [Authorize(Roles = "HRManager,Finance")]
        public IActionResult GetAll()
        {
            return Content("HRManager || Finance");
        }

        [Authorize(Roles = "PowerUser")]
        [Authorize(Roles = "ControlPanelUser")]
        public IActionResult Remove()
        {
            return Content("PowerUser && ControlPanelUser");
        }

        [Authorize(Policy = "1In3")] //policy định nghĩa ở Program
        public IActionResult Check()
        {
            return Content("Administrator || PowerUser || BackupAdministrator");
        }
    }
}