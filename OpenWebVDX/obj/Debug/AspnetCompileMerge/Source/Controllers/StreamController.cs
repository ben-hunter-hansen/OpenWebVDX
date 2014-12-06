using OpenWebVDX.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utils;
using VideoHandler;

namespace OpenWebVDX.Controllers
{
    public class StreamController : Controller
    {
        public ActionResult StreamView(string id)
        {
            if(RequestValidation.HasNullParams(id))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Video requestedVid = DatabaseOps.GetVideoRecord(id);
                System.Diagnostics.Debug.WriteLine("Video Title: " + requestedVid.Title);
                return View(requestedVid);
            }
        }

        [HttpGet]
        [ActionName("GetVideoStream")]
        public void ProcessStream(string id)
        {
            string filepath = DatabaseOps.GetVideoPath(id);
            string filename = id.Replace(" ", "_");
            HttpContextBase ctx = this.HttpContext;

            VideoStream vStream = new VideoStream(filepath, filename, ctx, Request, Response);
            vStream.OpenStream();
        }
    }
}
