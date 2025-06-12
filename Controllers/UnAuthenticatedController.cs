using Microsoft.AspNetCore.Mvc;

namespace ViewMyAssetDev.Controllers
{
    public class UnAuthenticatedController : Controller
    {
        public IActionResult UnAuthenticatedMain()
        {
            return View();
        }
    }
}
