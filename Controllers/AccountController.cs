using Microsoft.AspNetCore.Mvc;

namespace music_shed.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult AccessDenied(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }
}