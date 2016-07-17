using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServices
{
    public class Folder
    {
        public string Name { get; set; }
        public Folder()
        {
        }
        public Folder(string name)
        {
            this.Name = name;
        }
    }
}