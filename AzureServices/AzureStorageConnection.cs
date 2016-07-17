using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace AzureServices
{
    public class AzureStorageConnection
    {
        CloudStorageAccount storageAccount;
        CloudBlobClient blobClient;
        CloudBlobContainer container;
        public AzureStorageConnection()
        {
            this.storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            this.blobClient = storageAccount.CreateCloudBlobClient();
        }
        public CloudBlobContainer Container
        {
            get { return container; }
            set { container = this.blobClient.GetContainerReference("mycontainer"); }
        }
    }
}
