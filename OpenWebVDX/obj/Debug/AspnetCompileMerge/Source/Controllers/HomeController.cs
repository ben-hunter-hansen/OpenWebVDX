﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

using OpenWebVDX.API.FileHandler;
using Utils;
using Newtonsoft.Json.Linq;
using OpenWebVDX.Models;
using OpenWebVDX.API;
using System.Net.Http;
using System.Net.Http.Headers;

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
            VideoList vdi = DatabaseOps.AllVideoInfo();
            return Json(vdi, JsonRequestBehavior.AllowGet);
        }
    } 
}
