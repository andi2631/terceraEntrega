using Protocol.BuisnessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class FileHandler
    {
        public FileLogic filelogic;

        public FileHandler()
        {
            filelogic = new FileLogic();
        }

        public byte[] Read(string path, long offset, int length)
        {
            if (filelogic.Exists(path))
            {
                var data = new byte[length];

                using var fs = new FileStream(path, FileMode.Open)
                {
                    Position = offset
                };

                var bytesRead = 0;
                while (bytesRead < length)
                {
                    var read = fs.Read(data, bytesRead, length - bytesRead);
                    if (read == 0)
                        throw new Exception("Error en la lectura del archivo");
                    bytesRead += read;
                }

                return data;
            }

            throw new Exception("El archivo no existe");
        }

        public void Write(string fileName, byte[] data)
        {
            FileMode mode = (filelogic.Exists(fileName)) ? FileMode.Append : FileMode.Create;
            using var fileStream = new FileStream(fileName, mode);

            fileStream.Write(data, 0, data.Length);
        }
    }
}
