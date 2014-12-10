using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Configuration;
using System.IO;

namespace VDXApp.CloudService
{

    public static class VDXCloudAction
    {
        public const string READ = "READ";
        public const string WRITE = "WRITE";
        public const string STREAM = "STREAM";

        public static bool Read(CloudBlockBlob blob, Stream fileStream)
        {
            try
            {
                blob.DownloadToStream(fileStream);
                return true;
            }
            catch(Exception e)
            {
                CloudLogError.ErrorMsg(e.Message);
                return false;
            }
        }

        public static bool Write(CloudBlockBlob blob, Stream fileStream)
        {
            try
            {
                blob.UploadFromStream(fileStream);
                return true;
            }
            catch(Exception e)
            {
                CloudLogError.ErrorMsg(e.Message);
                return false;
            }
        }
    }

    public class VDXCloudBlob
    {
        private const string VIDEO_CONTAINER = "uploads";

        private CloudStorageAccount storageAccount;
        private CloudBlobClient client;
        private CloudBlobContainer container;
        private CloudBlockBlob blob;
        private string blobFileName;

        public CloudBlockBlob Blob { get { return blob; } }

        public VDXCloudBlob(string file_name)
        {
            this.storageAccount = getStorageAccount();
            this.client = this.storageAccount.CreateCloudBlobClient();
            this.container = this.client.GetContainerReference(VIDEO_CONTAINER);
            this.blobFileName = file_name;
            this.blob = this.container.GetBlockBlobReference(this.blobFileName);
        }

        public bool ExecuteActionLocal(string action_type, string local_file_uri)
        {
            if(action_type.Equals(VDXCloudAction.READ))
            {
                var stream = System.IO.File.OpenRead(@local_file_uri);
                bool actionResult = VDXCloudAction.Read(this.blob, stream);
                return actionResult;
            }
            else if(action_type.Equals(VDXCloudAction.WRITE))
            {
                var stream = System.IO.File.OpenRead(@local_file_uri);
                bool actionResult = VDXCloudAction.Write(this.blob, stream);
                return actionResult;
            }
            else
            {
                return false;
            }
        }

        public Stream ExecuteActionStream(string action_type)
        {
            if (action_type.Equals(VDXCloudAction.READ))
            {
                return this.Blob.OpenRead();
            }
            else if (action_type.Equals(VDXCloudAction.WRITE))
            {
                return this.Blob.OpenWrite();
            }
            else
            {
                return null;
            }
        }

        public string GetBlobPath()
        {
            return this.blob.Uri.AbsoluteUri;
        }

        private CloudStorageAccount getStorageAccount()
        {
            return CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
        }
    }
}

public static class CloudLogError
{
    public static void ErrorMsg(string msg)
    {
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
        // Create the blob client. 
        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        // Retrieve reference to a previously created container.
        CloudBlobContainer container = blobClient.GetContainerReference("errors");
        // Retrieve reference to a blob named "myblob".
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(msg);
        blockBlob.UploadText(msg);
    }
}