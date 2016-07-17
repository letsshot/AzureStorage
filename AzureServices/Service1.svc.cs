using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace AzureServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        //List folders available in the root folder
        public string ListFolder()
        {
            List<Folder> folders = new List<Folder>();
            //Get the folder's path
            var list = new AzureStorageConnection().Container.ListBlobs();
            foreach(IListBlobItem item in list){
                if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    string path = item.Uri.ToString();
                    int startIndex = path.IndexOf("mycontainer/")+12;
                    int endIndex = path.LastIndexOf("/");
                    string directory = path.Substring(startIndex, endIndex - startIndex);
                    folders.Add(new Folder(directory));
                }
            }
            return JsonConvert.SerializeObject(folders);
        }

        //List files available in a specific folder
        string ListFiles(string directory)
        {
            List<File> files = new List<File>();
            //get the files's path
            var list = new AzureStorageConnection().Container.GetDirectoryReference(directory).ListBlobs();
            foreach (IListBlobItem item in list)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    string path = item.Uri.ToString();
                    int startIndex = path.LastIndexOf("/") + 1;
                    string fileName = path.Substring(startIndex, path.Length - startIndex);
                    files.Add(new File(fileName));
                }
            }
            return JsonConvert.SerializeObject(files); ;
        }

        //Upload a document on a specific folder (forbidden to upload in root folder)
        //If this document is a Zip file, you should unzip it in the current folder and store its contents in a folder named as the Zip file
        void UploadDocument(string storageDirectory, string filePath)
        {

            //get the file's extension
            string fileExtension  = filePath.Substring(filePath.LastIndexOf(".")+1, filePath.Length -filePath.LastIndexOf(".")-1);
            //get the file's name with extension
            int startIndex = filePath.LastIndexOf("/") + 1;
            string fileNameWithExtension = filePath.Substring(startIndex, filePath.Length - startIndex);
            string fileNameWithoutExtension = filePath.Substring(filePath.LastIndexOf("/") + 1, filePath.LastIndexOf(".") - filePath.LastIndexOf("/") - 1);
            //verify the file extension is zip or not
            if(fileExtension == "zip")
            {
                System.IO.Directory.CreateDirectory(@"C:/" + fileNameWithoutExtension);
                ZipFile.ExtractToDirectory(@filePath, @"C:/" + fileNameWithoutExtension);
                foreach (ZipArchiveEntry entry in ZipFile.OpenRead(filePath).Entries)
                {
                    CloudBlockBlob blockblob = new AzureStorageConnection().Container.GetDirectoryReference(storageDirectory).GetBlockBlobReference(fileNameWithoutExtension + "\\" + entry.Name);
                    blockblob.UploadFromFile(@"C:/" + fileNameWithoutExtension + "\\" + entry.Name, FileMode.OpenOrCreate);
                }
            }else{
                //get or create a directory and a blockblob 
                CloudBlockBlob blob = new AzureStorageConnection().Container.GetDirectoryReference(storageDirectory).GetBlockBlobReference(fileNameWithExtension);
                //upload an local file into the azure storage
                blob.Properties.ContentType = "text/plain";
                using (var fileStream = System.IO.File.OpenRead(@filePath))
                {
                    blob.UploadFromStream(fileStream);
                }
            }
        }

        //Download a specific file
        //Or download a Zip file
        void DownloadDocument(string filePath, string destination)
        {
                //filePath = FolderName + "/" + fileName + "." + fileExtension
                CloudBlockBlob blob = new AzureStorageConnection().Container.GetBlockBlobReference(filePath);
                var fileNameWithExtension = filePath.Substring(filePath.LastIndexOf("/") + 1, filePath.Length - filePath.LastIndexOf("/"));
                System.IO.Directory.CreateDirectory(@destination);
                blob.DownloadToFile(@destination + "/" + fileNameWithExtension, FileMode.OpenOrCreate);
        }

        void ZipEntireDirectory(string directoryURI)
        {
            CloudBlobContainer container = new AzureStorageConnection().Container;
            string directory = directoryURI.Substring(directoryURI.IndexOf("mycontainer/") + 12, directoryURI.Length - directoryURI.IndexOf("mycontainer") - 12); 
            string directoryName = directory.Substring(0, directory.LastIndexOf("/"));
            TraverseDirectory(directoryURI, container, directory);
            ZipFile.CreateFromDirectory(@"C:\" + directoryName, @"C:\" + directoryName + ".zip");
            CloudBlockBlob blockblob = container.GetDirectoryReference("archives").GetBlockBlobReference(directoryName + ".zip");
            blockblob.UploadFromFile(@"C:\" + directoryName + ".zip", FileMode.OpenOrCreate);
        }
        void TraverseDirectory(string URI, CloudBlobContainer container, string directory)
        {
            CloudBlobDirectory sourcesFolder = container.GetDirectoryReference(directory);
            Directory.CreateDirectory(@"C:\" + directory);
            foreach (IListBlobItem item in sourcesFolder.ListBlobs())
            {
                if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    URI = item.Uri.ToString();
                    directory = URI.Substring(URI.IndexOf("mycontainer/") + 12, URI.Length - URI.IndexOf("mycontainer") - 12);
                    TraverseDirectory(URI, container, directory);
                }
                else if (item.GetType() == typeof(CloudBlockBlob))
                {
                    string filePath = item.Uri.ToString();
                    int startIndex = filePath.IndexOf(directory) + directory.Length;
                    string filePathWithExtensionInDirectory = filePath.Substring(startIndex, filePath.Length - startIndex);
                    Console.Write(filePathWithExtensionInDirectory);
                    Console.ReadKey();
                    CloudBlockBlob blockblob = (CloudBlockBlob)sourcesFolder.GetBlockBlobReference(filePathWithExtensionInDirectory);
                    Directory.CreateDirectory(@"C:\" + directory);
                    blockblob.DownloadToFile(@"C:\" + directory + "\\" + filePathWithExtensionInDirectory, FileMode.OpenOrCreate);
                }
            }

        }
        
    }
}
