using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.Services
{
    public class FileResolveService
    {

        internal const string ApiAddress = "http://192.168.0.15:5020/";
        internal const string contentWebRoot = "wwwroot";
        public string ResolvePath(string path)
        {
            string pathToEdit = path;
            pathToEdit = pathToEdit.Remove(0, pathToEdit.IndexOf(contentWebRoot));
            pathToEdit = pathToEdit.Replace(contentWebRoot + "\\", "");
            pathToEdit = pathToEdit.Replace("\\", "/");
            pathToEdit = Path.Combine(ApiAddress, pathToEdit);
            return pathToEdit;
        }
    }
}
