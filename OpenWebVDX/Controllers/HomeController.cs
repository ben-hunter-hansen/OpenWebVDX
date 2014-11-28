using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

using OpenWebVDX.API.FileHandler;
using Utils;
using Newtonsoft.Json.Linq;
using OpenWebVDX.Models;

namespace OpenWebVDX.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ActionName("GetIndexVids")]
        public JsonResult getVids()
        {
            VideoInfo vdi = DatabaseOps.AllVideoInfo();
            return Json(vdi, JsonRequestBehavior.AllowGet);
            //This whole thing sucks and doesn't work.
        }
    }
}
