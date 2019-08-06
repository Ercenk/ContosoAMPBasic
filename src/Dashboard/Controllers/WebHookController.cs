using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers
{
    public class WebHookController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}