using Newtonsoft.Json.Linq;
using OpenWebVDX.API.FileHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            

            HttpPostedFileBase file = Request.Files[0];

            VDXFile vdxFile = new VDXFile(file, videoTitle, "admin", DateTime.Now.ToString());

            if(!VDXFileValidator.isVideoFormat(vdxFile))
            {
                return Json("Upload failed: That is not a video.");
            }
            else
            {
                bool uploadStatus = vdxFile.writeUpload(this.HttpContext);
                if(uploadStatus)
                {
                    Task.Factory.StartNew(() =>
                    {
                        VDXFile toConvert = vdxFile;
                        toConvert.executeConversion();

                    }); 
                    return Json(file.FileName + " has been uploaded successfully!");
                }
                else
                {
                    return Json("Upload failed: The server was unable to process the request.");
                }
            }
        }
    }
}
