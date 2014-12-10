using OpenWebVDX.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VDXApp.StreamService;
using VDXApp.Utils;

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
            Video video = DatabaseOps.GetVideoRecord(id);
            HttpContextBase ctx = this.HttpContext;
            VideoStream vStream = new VideoStream(video, ctx, Request, Response);
            vStream.OpenStream();
        }
    }
}
