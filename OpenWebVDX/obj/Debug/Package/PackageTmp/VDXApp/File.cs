using CloudService;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using MySql.Data.MySqlClient;
using NReco.VideoConverter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Windows;
using Utils;

namespace OpenWebVDX.API.FileHandler
{

    public static class VDXMediaConverterFactory
    {
        public static void CreateInstance(VDXFile file)
        {
            Task.Factory.StartNew(() =>
            {
                VDXMediaConverter NewInstance = new VDXMediaConverter(file);
                NewInstance.RunThread();
            }); 
        }
    }

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
        public HttpPostedFileBase File { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string User { get; set; }
        public string Date { get; set; }
        public string OrigPath { get; set; }
        public string DestPath { get; set; }
        public string EncodingType { get; set; }

        public VDXFile(HttpPostedFileBase file, string title, string user, string date)
        {
            this.File = file;
            this.Title = title;
            this.FileName = fileNameFromTitle(this.Title) + ".mp4";
            this.User = user;
            this.Date = date;
            this.EncodingType = getMyEncoding();
        }

        public bool writeUpload(HttpContextBase context)
        {

            int fileSize = File.ContentLength;
            string originalFileName = File.FileName; //Original file
            string mimeType = File.ContentType;

            System.IO.Stream fileContent = File.InputStream;

            string pathToUpload = context.Server.MapPath("~/App_Data/uploads/") + originalFileName;
            string destinationPath = context.Server.MapPath("~/App_Data/uploads/") + this.FileName;

            try
            {
                File.SaveAs(pathToUpload);

                this.OrigPath = pathToUpload;
                this.DestPath = destinationPath;
                this.EncodingType = getMyEncoding();
                return true;
            }
            catch (StorageException se)
            {
                System.Diagnostics.Debug.WriteLine("VDXFile: Error in writeUpload() > " + se.StackTrace + "\n" + se.Message);
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("VDXFile: Error in writeUpload() > " + ex.Message);
                return false;
            }
        }


        private string fileNameFromTitle(string title)
        {
            string ret = title;
            if (ret.Contains(" "))
            {
                ret = ret.Replace(" ", "_");
            }
            return ret;
        }

        private string getMyEncoding()
        {
            string contentType = this.File.ContentType;

            for (int i = 0; i < contentType.Length; i++)
            {
                if (contentType[i] == '/')
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

        const string TARGET_ENCODE = Format.matroska;

        protected string orig_path;
        protected string dest_path;
        protected string file_name;
        protected string encoding;
        protected string title;
        protected string date;
        protected string user;

        //Integer representing that status of media conversion
        public int ConversionStatus { get; set; }

        public VDXMediaConverter(VDXFile media)
        {
            this.orig_path = media.OrigPath;
            this.dest_path = media.DestPath;
            this.file_name = media.FileName;
            this.encoding = media.EncodingType;
            this.title = media.Title;
            this.user = media.User;
            this.date = media.Date;
        }

        private int AttemptConversion()
        {
            int status = WORKING;

            FFMpegConverter converter = new FFMpegConverter();

            try
            {
                converter.ConvertMedia(this.orig_path, this.dest_path, TARGET_ENCODE);
                status = SUCCESS;
            }
            catch (FFMpegException e)
            {
                System.Diagnostics.Debug.WriteLine("Media Converter Failed: " + e.Message);
                status = FAILURE;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("VDXMediaConverter Error: " + e.Message);
                status = FAILURE;
            }
            converter.Abort();
            return status;
        }

        public override void RunThread()
        {
            int attempt = WORKING;
            while(attempt == WORKING)
            {
                attempt = AttemptConversion();
            }

            if(attempt == FAILURE)
            {
                System.Diagnostics.Debug.WriteLine("Unable to convert media.");
            }
            else if(attempt == SUCCESS)
            {
                System.Diagnostics.Debug.WriteLine("Media converted successfully.");
                if (VDXApplicationSettings.Enviornment.Equals(VDXApplicationSettings.PRODUCTION))
                {
                    VDXCloudBlob storageService = new VDXCloudBlob(this.file_name); 
                    if (storageService.ExecuteActionLocal(VDXCloudAction.WRITE, this.dest_path))
                    {
                        string blob_uri = storageService.GetBlobPath();
                        DatabaseOps.UploadNotification(this.title, this.user, this.date, blob_uri);
                    }
                }
                else
                {
                    DatabaseOps.UploadNotification(this.title, this.user, this.date, this.dest_path);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Something went horribly wrong.");
            }

            this.ConversionStatus = attempt;
            Abort();
        }
    }

    public static class VDXFileValidator
    {
        public static bool isVideoFormat(VDXFile toValidate)
        {
            if (toValidate.File.ContentType.Contains("video"))
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