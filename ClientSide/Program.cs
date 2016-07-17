using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSide
{
    class Program
    {
        public static void TraverseDirectory(string URI,CloudBlobContainer container,string directory)
        {
            CloudBlobDirectory sourcesFolder = container.GetDirectoryReference(directory);
            Directory.CreateDirectory(@"C:\" + "mycontainer" + "\\"+ directory);
            foreach (IListBlobItem item in sourcesFolder.ListBlobs())
            {
                if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    URI = item.Uri.ToString();
                    Console.Write(URI);
                    Console.ReadKey();
                    directory = URI.Substring(URI.IndexOf("mycontainer/")+12, URI.Length - URI.IndexOf("mycontainer")-12);
                    Console.Write(directory);
                    Console.ReadKey();
                    TraverseDirectory(URI, container, directory);
                }else if(item.GetType() == typeof(CloudBlockBlob)){
                    string filePath = item.Uri.ToString();
                    int startIndex = filePath.IndexOf(directory) + directory.Length;
                    string filePathWithExtensionInDirectory = filePath.Substring(startIndex, filePath.Length - startIndex);
                    CloudBlockBlob blockblob = (CloudBlockBlob)sourcesFolder.GetBlockBlobReference(filePathWithExtensionInDirectory);
                    Directory.CreateDirectory(@"C:\" + "mycontainer"+ "\\" + directory);
                    blockblob.DownloadToFile(@"C:\" + "mycontainer" + "\\" + directory + "\\" + filePathWithExtensionInDirectory, FileMode.OpenOrCreate);
                }
            }

        }
        static void Main(string[] args)
        {
            //try
            //{

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");
                string rootFolder = container.Uri.ToString();
                string directory = rootFolder.Substring(rootFolder.IndexOf("mycontainer"), rootFolder.Length - rootFolder.IndexOf("mycontainer"));
                var list = container.ListBlobs();
                foreach(IListBlobItem item in list){
                    if (item.GetType() == typeof(CloudBlobDirectory))
                    {
                        string path = item.Uri.ToString();
                        int startIndex = path.IndexOf("mycontainer/") + 12;
                        int endIndex = path.LastIndexOf("/");
                        string folderDirectory = path.Substring(startIndex, endIndex - startIndex+1);
                        TraverseDirectory(path, container, folderDirectory);
                    }
                    else if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        string filePath = item.Uri.ToString();
                        int startIndex = filePath.IndexOf("mycontainer")+12;
                        string filePathWithExtensionInDirectory = filePath.Substring(startIndex, filePath.Length - startIndex);
                        Directory.CreateDirectory(@"C:\" + "mycontainer");
                        CloudBlockBlob blockblob = (CloudBlockBlob)item.Container.GetBlockBlobReference(filePathWithExtensionInDirectory);
                        blockblob.DownloadToFile(@"C:\" + "mycontainer" + "\\" + filePathWithExtensionInDirectory, FileMode.OpenOrCreate);
                    }
                }
                ZipFile.CreateFromDirectory(@"C:\" + "mycontainer", @"C:\Users\16728_000\Documents\" + "mycontainer" + ".zip");
                CloudBlockBlob backupBlockblob = blobClient.GetContainerReference("backups").GetBlockBlobReference("mycontainer" + ".zip");
                backupBlockblob.UploadFromFile(@"C:\Users\16728_000\Documents\" + "mycontainer" + ".zip", FileMode.OpenOrCreate);

                
            /*string rootFolder = container.Uri.ToString();
            string directory = rootFolder.Substring(rootFolder.IndexOf("mycontainer"), rootFolder.Length - rootFolder.IndexOf("mycontainer"));
            string directoryName = directory.Substring(0, directory.LastIndexOf("/")); 
            Console.Write(directoryName);
            Console.ReadKey();
            TraverseDirectory(sourcesFolder, container, directory);
            ZipFile.CreateFromDirectory(@"C:\" + directoryName, @"C:\Users\16728_000\Documents\" + directoryName + ".zip");
            Console.ReadKey();
            CloudBlockBlob blockblob = blobClient.GetContainerReference("backups").GetBlockBlobReference(directoryName + ".zip");
            blockblob.UploadFromFile(@"C:\Users\16728_000\Documents\" + directoryName + ".zip", FileMode.OpenOrCreate);
            Console.ReadKey(); */


            /*  CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
              CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
              CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");
              string URI = "http://127.0.0.1:10000/devstoreaccount1/mycontainer/test1/";
              string directory = URI.Substring(URI.IndexOf("mycontainer/") + 12, URI.Length - URI.IndexOf("mycontainer")- 12); //test/
              string directoryName =directory.Substring(0, directory.LastIndexOf("/"));
              Console.Write(directoryName);
              Console.ReadKey();
              TraverseDirectory(URI, container, directory);
              ZipFile.CreateFromDirectory(@"C:\" + directoryName, @"C:\Users\16728_000\Documents\" + directoryName + ".zip");
              Console.ReadKey();
              CloudBlockBlob blockblob = container.GetDirectoryReference("archives").GetBlockBlobReference(directoryName + ".zip");
              blockblob.UploadFromFile(@"C:\Users\16728_000\Documents\" + directoryName + ".zip", FileMode.OpenOrCreate);
              Console.ReadKey(); */

              /*CloudBlobDirectory sourcesFolder = container.GetDirectoryReference(directory);
                Directory.CreateDirectory(@"C:\" + directory);
                foreach (IListBlobItem item in sourcesFolder.ListBlobs())
                {
                    if (item.GetType() == typeof(CloudBlobDirectory))
                    {
                        URI = item.Uri.ToString();
                        directory = URI.Substring(URI.IndexOf("mycontainer/") + 12, URI.Length - URI.IndexOf("mycontainer") - 12);
                        Console.Write(directory);
                        Console.ReadKey();
                    }
                    string filePath = item.Uri.ToString();
                    int startIndex = filePath.IndexOf(directory) + directory.Length;
                    string filePathWithExtensionInDirectory = filePath.Substring(startIndex, filePath.Length - startIndex);
                    CloudBlockBlob blockblob = (CloudBlockBlob)sourcesFolder.GetBlockBlobReference(filePathWithExtensionInDirectory);
                    Directory.CreateDirectory(@"C:\" + directory);
                    blockblob.DownloadToFile(@"C:\" + directory + "\\" + filePathWithExtensionInDirectory, FileMode.OpenOrCreate); 
                    Console.Write(item.Uri);
                    Console.ReadKey(); 
                } */
                
             /*   string filePath = "C:/zipfile.zip";
                string fileExtension = filePath.Substring(filePath.LastIndexOf(".") + 1, filePath.Length - filePath.LastIndexOf(".")-1);
                string fileNameWithoutExtension = filePath.Substring(filePath.LastIndexOf("/")+1, filePath.LastIndexOf(".") - filePath.LastIndexOf("/")-1);
                System.IO.Directory.CreateDirectory(@"C:/" + fileNameWithoutExtension);
                ZipFile.ExtractToDirectory(@filePath, @"C:/" + fileNameWithoutExtension);
                foreach (ZipArchiveEntry entry in ZipFile.OpenRead(filePath).Entries)
                {
                    CloudBlockBlob blockblob = container.GetDirectoryReference("test1").GetBlockBlobReference(fileNameWithoutExtension + "\\" + entry.Name);
                    blockblob.UploadFromFile(@"C:/" + fileNameWithoutExtension + "\\" + entry.Name, FileMode.OpenOrCreate);
                }

                Console.Write(fileNameWithoutExtension);
                Console.ReadKey(); */
                
                //blob.DownloadToFile("C:/Users/16728_000/Documents/zipfile.zip", FileMode.OpenOrCreate);
                //Console.Write(blob);
                //Console.ReadKey();
           // }
           // catch (Microsoft.WindowsAzure.Storage.StorageException e)
           // {
          //  }

            /*string path = "http://127.0.0.1:10000/devstoreaccount1/mycontainer/test1/zipfile.zip";
            string fileDirectory = path.Substring(0, path.LastIndexOf("."));
            Console.Write(fileDirectory);
            Console.ReadKey();
            ZipFile zipFile = ZipFile.Read(path);
            zipFile.ExtractAll(fileDirectory, ExtractExistingFileAction.OverwriteSilently); */

            
            //CloudBlockBlob blob = container.GetDirectoryReference("test1").GetBlockBlobReference(fileName);
            //blob.Properties.ContentType = "text/plain";
    
            /*using (var fileStream = System.IO.File.OpenRead(@path))
            {
                blob.UploadFromStream(fileStream);
            }*/
            
            /*var blob = container.GetBlobReference("test.jpg");
            Console.Write(blob.Uri);
            Console.ReadKey();
            */
            //List<File> files = new List<File>();
            //var list = container.GetDirectoryReference("test1").ListBlobs();

            //List<Folder> folders = new List<Folder>();
            /*foreach (IListBlobItem item in list)
            {
                var type = item.GetType();
               if (item.GetType() == typeof(CloudBlockBlob))
                {
                    var path = item.Uri.ToString();
                    int startIndex = path.LastIndexOf("/") + 1;
                    string fileName = path.Substring(startIndex, path.Length - startIndex);
                    Console.Write(fileName);
                    Console.ReadKey();
                    files.Add(new File(fileName));
                } */
                //else if (type == typeof(CloudBlobDirectory)) {
                    //string path = item.Uri.ToString();
                    //int i = path.IndexOf("mycontainer/")+12;
                    //int j = path.LastIndexOf("/");
                    //var directory = path.Substring(i, j-i);
                    //folders.Add(new Folder(directory));

                
            
            //Console.Write(JsonConvert.SerializeObject(files));
            //Console.ReadKey();
        }
    }
}
