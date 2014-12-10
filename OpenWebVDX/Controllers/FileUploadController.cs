using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebSockets;
using VDXApp.FileService;
using VDXApp.Utils;
using VDXApp.WebSocketService;


namespace OpenWebVDX.Controllers
{
    public class FileUploadController : Controller
    {
        //
        // GET: /FileUpload/
        string NAME = "";
        string TITLE = "";
        string TYPE = "";

        [HttpGet]
        [ActionName("Upload")]
        public ActionResult GetUploadView()
        {
            return View();
        }

        [HttpGet]
        [ActionName("SocketUpload")]
        public void SocketUpload(string name,string title, string type)
        {
            System.Diagnostics.Debug.WriteLine("Web Socket Request Received with param "+name);
            try
            {
                if (ControllerContext.HttpContext.IsWebSocketRequest)
                {
                    NAME = name;
                    TITLE = title;
                    TYPE = type;
                    HttpContext.AcceptWebSocketRequest(BeginUpload);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public async Task BeginUpload(AspNetWebSocketContext context)
        {
            const int msg_size = 8192;
            bool success = false;

            WebSocket socket = context.WebSocket;
            ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[msg_size]);
            WebSocketReceiveResult result = null;

            try
            {

                string f_path = context.Server.MapPath("~/App_Data/uploads/");
                using (var fs = new System.IO.FileStream(f_path + NAME, System.IO.FileMode.Create,
                             System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
                {
                    int bytes_up = 0;
                    do
                    {
                        result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                        fs.Write(buffer.Array, buffer.Offset, result.Count);
                        bytes_up += result.Count;

                        //A KB of data has uploaded, notify client
                        if (bytes_up % 1024 == 0)
                        {
                            int kb = bytes_up / 1024;
                            int mb = kb / 1024;
                            string bytesReadStr = mb.ToString();

                            Byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(bytesReadStr);

                            await socket.SendAsync(new ArraySegment<byte>(sendBytes),
                                WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                    while (!result.EndOfMessage);

                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Video successfully uploaded.", CancellationToken.None);
                    success = true;

                    HttpPostedFileBase file = new MemoryFile(fs, TYPE, NAME);
                    VDXSocketFile vdxFile = new VDXSocketFile(file, TITLE, "admin", DateTime.Now.ToString());
                    vdxFile.PrepareForConversion(f_path, "mp4");
                    VDXMediaConverterFactory.CreateInstance(vdxFile);
                }
            }
            catch(Exception e)
            {
                success = false;
            }

            if (success)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Upload Complete!.", CancellationToken.None);
            }
            else
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Upload Failed.", CancellationToken.None);
            }
        }
    }
}
