using Newtonsoft.Json.Linq;
using OpenWebVDX.API.FileHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utils;

namespace OpenWebVDX.Controllers
{
    public class FileUploadController : Controller
    {
        //
        // GET: /FileUpload/

        [HttpGet]
        [ActionName("Upload")]
        public ActionResult GetUploadView()
        {
            return View();
        }

        [HttpPost]
        [ActionName("UploadRequest")]
        public JsonResult UploadFile()
        {
            string titleJson = Request.Form.ToString();
            string videoTitle = StringOps.ExtractJsonValue(titleJson);
            
            System.Diagnostics.Debug.WriteLine(videoTitle);

            HttpPostedFileBase file = Request.Files[0];

            VDXFile vdxFile = new VDXFile(file, videoTitle);
            if(!VDXFileValidator.isVideoFormat(vdxFile))
            {
                return Json("Upload failed: That is not a video.");
            }
            else
            {
                vdxFile.writeUpload(this.HttpContext, "admin", DateTime.Now.ToString());
                return Json(file.FileName + " has been uploaded successfully!");
            }
        }
    }
}
