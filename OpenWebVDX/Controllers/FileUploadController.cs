using Newtonsoft.Json.Linq;
using OpenWebVDX.API.FileHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebSockets;
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

        [HttpGet]
        [ActionName("SocketUpload")]
        public JsonResult SocketUpload()
        {
            System.Diagnostics.Debug.WriteLine("Web Socket Request Received");
            if (ControllerContext.HttpContext.IsWebSocketRequest)
            {
                
                HttpContext.AcceptWebSocketRequest(BeginUpload);   
                return Json("Socket established.");
            }
            else
            {
                return Json("Unable to process socket handshake");
            }
        }

        public async Task BeginUpload(AspNetWebSocketContext context)
        {
            WebSocket socket = context.WebSocket;
            ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[8192]);

            WebSocketReceiveResult result = null;

            using (var ms = new MemoryStream())
            {
                do
                {
                    result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                HttpPostedFileBase file = new MemoryFile(ms, "video/mp4","socketFile");
                VDXFile vdxFile = new VDXFile(file, "socket_file", "admin", DateTime.Now.ToString());
                bool uploadStatus = vdxFile.writeUpload(this.HttpContext);
                if (uploadStatus)
                {
                    VDXMediaConverterFactory.CreateInstance(vdxFile);
                }
            }
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
                    VDXMediaConverterFactory.CreateInstance(vdxFile);
                    return Json(
                        "Your video has been uploaded successfully "+
                        "and will be available for streaming shortly.");
                }
                else
                {
                    return Json("Upload failed: The server was unable to process the request.");
                }
            }
        }
    }
}
