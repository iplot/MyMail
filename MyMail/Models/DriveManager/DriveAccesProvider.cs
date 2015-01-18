using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using MyMail.Models.Entities;

namespace MyMail.Models.DriveManager
{
    public class DriveAccesProvider : IDriveAccesProvider
    {
        private readonly string _folderPath;

        public DriveAccesProvider(string rootFolderPath)
        {
            _folderPath = rootFolderPath;

            if(!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);
        }

        public string addAccountFolder(string accountEmail)
        {
            string path = Path.Combine(_folderPath, accountEmail);

            IEnumerable<string> states = Enum.GetNames(typeof (State));

            foreach (string state in states)
            {
                Directory.CreateDirectory(Path.Combine(path, state));
            }

            return path;
        }
    }
}