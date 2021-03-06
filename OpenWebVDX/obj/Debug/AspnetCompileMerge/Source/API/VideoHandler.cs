﻿using System;
using System.IO;
using System.Web;
namespace VideoHandler
{
    public class VideoStream
    {
        private string filepath;
        private string filename;
        private HttpContextBase ctx;
        private HttpRequestBase Request;
        private HttpResponseBase Response;

        public VideoStream(string filepath, string filename, HttpContextBase ctx, HttpRequestBase Request, HttpResponseBase Response)
        {
            this.filepath = filepath;
            this.filename = filename;
            this.ctx = ctx;
            this.Request = Request;
            this.Response = Response;
        }

        public void OpenStream()
        {
            System.IO.Stream iStream = null;
            byte[] buffer = new Byte[4096];
            int length;
            long dataToRead;

            try
            {
                // Open the file.
                iStream = new System.IO.FileStream(filepath, System.IO.FileMode.Open,
                            System.IO.FileAccess.Read, System.IO.FileShare.Read);


                // Total bytes to read:
                dataToRead = iStream.Length;

                Response.AddHeader("Accept-Ranges", "bytes");
                Response.ContentType = "video/mp4";

                int startbyte = 0;

                if (!String.IsNullOrEmpty(Request.Headers["Range"]))
                {
                    string[] range = Request.Headers["Range"].Split(new char[] { '=', '-' });
                    startbyte = Int32.Parse(range[1]);
                    iStream.Seek(startbyte, SeekOrigin.Begin);

                    Response.StatusCode = 206;
                    Response.AddHeader("Content-Range", String.Format(" bytes {0}-{1}/{2}", startbyte, dataToRead - 1, dataToRead));
                }

                while (dataToRead > 0)
                {
                    // Verify that the client is connected.
                    if (Response.IsClientConnected)
                    {
                        // Read the data in buffer.
                        length = iStream.Read(buffer, 0, buffer.Length);

                        // Write the data to the current output stream.
                        Response.OutputStream.Write(buffer, 0, buffer.Length);
                        // Flush the data to the HTML output.
                        Response.Flush();

                        buffer = new Byte[buffer.Length];
                        dataToRead = dataToRead - buffer.Length;
                    }
                    else
                    {
                        //prevent infinite loop if user disconnects
                        dataToRead = -1;
                    }
                }
            }
            catch (HttpException htx)
            {
                System.Diagnostics.Debug.WriteLine("HttpException: " + htx.Message);
            }
            catch (Exception ex)
            {
                // Trap the error, if any.
                Response.Write("Error : " + ex.Message);
            }
            finally
            {
                if (iStream != null)
                {
                    //Close the file.
                    iStream.Close();
                }
                Response.Close();
            }
        }
    }
}