using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Everywhere_Admin_UI.Controllers
{
    public class VideoController : Controller
    {
        // GET: Video
        public ActionResult VideoSettings()
        {
            return View();
        }
    }
}