using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServices
{
    public class File
    {
        public string FileName { get; set; }
        public File()
        {

        }
        public File(string fileName)
        {
            this.FileName = fileName;
        }
    }
}