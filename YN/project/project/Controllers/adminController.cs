using Microsoft.AspNetCore.Mvc;

namespace project.Controllers
{
    public class adminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult login()
        {
            return View();
        }
    }
}
