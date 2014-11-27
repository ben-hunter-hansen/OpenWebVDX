using OpenWebVDX.API.FileHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            HttpPostedFileBase file = Request.Files[0]; //Uploaded file
            System.Diagnostics.Debug.WriteLine(file.GetType());
            System.Diagnostics.Debug.WriteLine(file.ContentLength);
            System.Diagnostics.Debug.WriteLine(file.ContentType);
            VDXFile vdxFile = new VDXFile(file);
            vdxFile.writeToAppData(this.HttpContext);
            return Json("Uploaded " + Request.Files.Count + " files");
        }
    }
}
