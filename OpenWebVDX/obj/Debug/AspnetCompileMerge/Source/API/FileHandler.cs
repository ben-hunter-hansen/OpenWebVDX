using MySql.Data.MySqlClient;
using NReco.VideoConverter;
using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Windows;
using Utils;

namespace OpenWebVDX.API.FileHandler
{
    public abstract class BaseThread
    {
        private Thread _thread;

        protected BaseThread() { _thread = new Thread(new ThreadStart(this.RunThread)); }

        // Thread methods / properties
        public void Start() { _thread.Start(); }
        public void Join() { _thread.Join(); }
        public void Abort() { _thread.Abort(); }
        public bool IsAlive { get { return _thread.IsAlive; } }

        // Override in base class
        public abstract void RunThread();
    }

   public class VDXFile
   {
       private HttpPostedFileBase file;
       private string title;
       private string user;
       private string date;
       private string origPath;
       private string destPath;
       private string encodingType;

       public HttpPostedFileBase File { get { return file; } }

       public VDXFile(HttpPostedFileBase file, string title, string user, string date)
       {
           this.file = file;
           this.title = title;
           this.user = user;
           this.date = date;
       }

       public bool writeUpload(HttpContextBase context)
       {

           int fileSize = file.ContentLength;
           string fileName = file.FileName;
           string mimeType = file.ContentType;

           System.IO.Stream fileContent = file.InputStream;

           string pathToUpload = context.Server.MapPath("~/App_Data/uploads/") + fileName;
           string destinationPath = context.Server.MapPath("~/App_Data/uploads/") + fileNameFromTitle(this.title) + ".mp4";

           try
           {
               file.SaveAs(pathToUpload);
               this.origPath = pathToUpload;
               this.destPath = destinationPath;
               this.encodingType = getMyEncoding();
               return true;
           }
           catch(Exception ex)
           {
               System.Diagnostics.Debug.WriteLine("VDXFile: Error in writeUpload() > " + ex.Message);
               return false;
           }
       }

       public void executeConversion()
       {
           string title = this.title;
           string user = this.user;
           string date = this.date;
           string origPath = this.origPath;
           string destPath = this.destPath;
           string encodingType = this.encodingType;
           VDXMediaConverter conversionThread = new VDXMediaConverter(title, date, user, origPath, destPath, encodingType);

           try
           {
               conversionThread.RunThread();
           }
           catch(Exception ex)
           {
               System.Diagnostics.Debug.WriteLine("Failed to execute file conversion: " + ex.Message);
           }
           finally
           {
               if(conversionThread.ConversionStatus == 1)
               {
                   System.Diagnostics.Debug.WriteLine("------ A FILE HAS BEEN CONVERTED SUCCESSFULLY --------");
               }
               else if(conversionThread.ConversionStatus == 2)
               {
                   System.Diagnostics.Debug.WriteLine("------ A FILE HAS FAILED TO CONVERT --------");
               }
           }
       }
       
       private string fileNameFromTitle(string title)
       {
           string ret = title;
           if(ret.Contains(" "))
           {
               ret = ret.Replace(" ", "_");
           }
           return ret;
       }

       private string getMyEncoding()
       {
           string contentType = this.file.ContentType;

           for(int i = 0; i < contentType.Length; i++)
           {
               if(contentType[i] == '/')
               {
                   contentType = contentType.Substring(i + 1);
                   break;
               }
           }

           return contentType;
       }
   }

   public class VDXMediaConverter : BaseThread
   {
       const int WORKING = 0;
       const int SUCCESS = 1;
       const int FAILURE = 2;

       const string ENCODING_ERROR = "Unable to adjust encoding type.";

       protected string orig_path;
       protected string dest_path;
       protected string encoding;
       protected string title;
       protected string date;
       protected string user;

       //Integer representing that status of media conversion
       public int ConversionStatus { get; set; }

       public VDXMediaConverter(string title, string date, string user, string orig_path, string dest_path, string encoding)
       {
           this.orig_path = orig_path;
           this.dest_path = dest_path;
           this.encoding = encoding;
           this.title = title;
           this.user = user;
           this.date = date;

           this.ConversionStatus = WORKING;
       }

       protected static string AdjustEncoding(string enc)
       {
           if(enc.Equals(Format.mp4))
           {
               return Format.webm;
           }
           else if(enc.Equals(Format.webm))
           {
               return ENCODING_ERROR; //No more encodings to try
           }
           else
           {
               return enc;
           }
       }

       public override void RunThread()
       {
           FFMpegConverter converter = new FFMpegConverter();
           ConvertSettings settings = new ConvertSettings();

           try
           {
               string o_path = this.orig_path;
               string d_path = this.dest_path;


               converter.ConvertMedia(o_path, d_path, Format.mp4);
           }
           catch (FFMpegException e)
           {
               System.Diagnostics.Debug.WriteLine("Error in VDXMediaConverter.ConvertMediaFile(): " + e.Message);
               this.ConversionStatus = FAILURE;

           }
           finally
           {
               if(this.ConversionStatus == FAILURE)
               {
                   System.Diagnostics.Debug.WriteLine("Unable to convert media.");
                   //TODO: Notify client
               }
               else
               {
                   System.Diagnostics.Debug.WriteLine("Media converted successfully.");

                   string t = this.title;
                   string u = this.user;
                   string d = this.date;
                   string d_path = this.dest_path;

                   DatabaseOps.UploadNotification(t, u, d, d_path);

                   this.ConversionStatus = SUCCESS;
               }

               Abort();
           }
       }
   }

   public static class VDXFileValidator
   {
       public static bool isVideoFormat(VDXFile toValidate)
       {
           if(toValidate.File.ContentType.Contains("video"))
           {
               return true;
           } 
           else
           {
               return false;
           }
       }
   }
}