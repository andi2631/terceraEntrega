using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.BuisnessLogic
{
    public class FileLogic
    {
        public bool Exists(string path)
        {
            if (File.Exists(path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetName(string path)
        {
            string noExiste = "No existe archivo";
            if (File.Exists(path))
            {
                return new FileInfo(path).Name;
            }
            else
            {
                return noExiste;
            }
        }
        public string GetPath(string fileName)
        {
            return Path.GetFullPath(fileName);
        }

        public long GetFileSize(string path)
        {
            long size = 0;
            if (File.Exists(path))
            {
                return new FileInfo(path).Length;
            }
            else
            {
                return size;
            }
        }
    }
}
