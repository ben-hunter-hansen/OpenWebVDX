using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using MySql.Data.MySqlClient;
using NReco.VideoConverter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Windows;
using VDXApp.WebSocketService;
using VDXApp.CloudService;
using VDXApp.Utils;

namespace VDXApp.FileService
{

    public static class VDXMediaConverterFactory
    {
        public static void CreateInstance(VDXSocketFile file)
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

    public static class Encoding
    {
        public static string FileExtension(string content_type)
        {
            string contentType = content_type;

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

        public VDXMediaConverter(VDXSocketFile media)
        {
            this.orig_path = media.OrigPath;
            this.dest_path = media.DestPath;
            this.file_name = media.TargetFileName;
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
                System.Diagnostics.Debug.WriteLine(this.orig_path + " : " + this.dest_path);
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
                if (!VDXApplicationSettings.IsLocalEnviornment())
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
}