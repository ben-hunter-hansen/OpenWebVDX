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
            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase file = Request.Files[i]; //Uploaded file
                VDXFile vdxFile = new VDXFile(file);
                vdxFile.writeToAppData(this.HttpContext);
            }
            return Json("Uploaded " + Request.Files.Count + " files");
        }
    }
}
