using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers
{
    [AllowAnonymous]
    public class StaticController : Controller
    {
        [Route("privacy")]
        public ActionResult Privacy()
        {
            return View();
        }

        [Route("support")]
        public ActionResult Support()
        {
            return View();
        }
    }
}