using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.IO.Compression;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        //
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");
                string rootFolder = container.Uri.ToString();
                string directory = rootFolder.Substring(rootFolder.IndexOf("mycontainer"), rootFolder.Length - rootFolder.IndexOf("mycontainer"));
                var list = container.ListBlobs();
                foreach (IListBlobItem item in list)
                {
                    if (item.GetType() == typeof(CloudBlobDirectory))
                    {
                        string path = item.Uri.ToString();
                        int startIndex = path.IndexOf("mycontainer/") + 12;
                        int endIndex = path.LastIndexOf("/");
                        string folderDirectory = path.Substring(startIndex, endIndex - startIndex + 1);
                        TraverseDirectory(path, container, folderDirectory);
                    }
                    else if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        string filePath = item.Uri.ToString();
                        int startIndex = filePath.IndexOf("mycontainer") + 12;
                        string filePathWithExtensionInDirectory = filePath.Substring(startIndex, filePath.Length - startIndex);
                        Directory.CreateDirectory(@"C:\" + "mycontainer");
                        CloudBlockBlob blockblob = (CloudBlockBlob)item.Container.GetBlockBlobReference(filePathWithExtensionInDirectory);
                        blockblob.DownloadToFile(@"C:\" + "mycontainer" + "\\" + filePathWithExtensionInDirectory, FileMode.OpenOrCreate);
                    }
                }
                ZipFile.CreateFromDirectory(@"C:\" + "mycontainer", @"C:\" + "mycontainer" + ".zip");
                CloudBlockBlob backupBlockblob = blobClient.GetContainerReference("backups").GetBlockBlobReference("mycontainer" + ".zip");
                backupBlockblob.UploadFromFile(@"C:\" + "mycontainer" + ".zip", FileMode.OpenOrCreate);

                await Task.Delay(60000);
            }
        }

        public void TraverseDirectory(string URI, CloudBlobContainer container, string directory)
        {
            CloudBlobDirectory sourcesFolder = container.GetDirectoryReference(directory);
            Directory.CreateDirectory(@"C:\" + "mycontainer" + "\\" + directory);
            foreach (IListBlobItem item in sourcesFolder.ListBlobs())
            {
                if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    URI = item.Uri.ToString();
                    Console.Write(URI);
                    Console.ReadKey();
                    directory = URI.Substring(URI.IndexOf("mycontainer/") + 12, URI.Length - URI.IndexOf("mycontainer") - 12);
                    Console.Write(directory);
                    Console.ReadKey();
                    TraverseDirectory(URI, container, directory);
                }
                else if (item.GetType() == typeof(CloudBlockBlob))
                {
                    string filePath = item.Uri.ToString();
                    int startIndex = filePath.IndexOf(directory) + directory.Length;
                    string filePathWithExtensionInDirectory = filePath.Substring(startIndex, filePath.Length - startIndex);
                    CloudBlockBlob blockblob = (CloudBlockBlob)sourcesFolder.GetBlockBlobReference(filePathWithExtensionInDirectory);
                    Directory.CreateDirectory(@"C:\" + "mycontainer" + "\\" + directory);
                    blockblob.DownloadToFile(@"C:\" + "mycontainer" + "\\" + directory + "\\" + filePathWithExtensionInDirectory, FileMode.OpenOrCreate);
                }
            }

        }

    }
}
