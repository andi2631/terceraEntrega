using Protocol.BuisnessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class Sender
    {
        private readonly FileLogic fileLogic;
        private readonly FileHandler fileHandler;

        public Sender()
        {
            this.fileLogic = new FileLogic();
            this.fileHandler = new FileHandler();
        }

        public async Task Send(ActionCode code, string message, TcpClient tcpClient)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            int dataLength = data.Length;

            byte[] headerData = this.HeaderCreator(code, dataLength);

            await BytesSender(headerData, tcpClient);    // Primero envia header con orden y largo de datos
            await BytesSender(data, tcpClient);          // Luego envia los datos (el mensaje)
        }

        private static async Task BytesSender(byte[] data, TcpClient tcpClient)
        {
            int size = data.Length;
            NetworkStream networkStream = tcpClient.GetStream();
            await networkStream.WriteAsync(data,0,size).ConfigureAwait(false);
        }

        public async Task SendFile(string path, TcpClient tcpClient)
        {
            if (fileLogic.Exists(path))
            {
                string fileName = this.fileLogic.GetName(path);
                byte[] headerData = this.HeaderCreator(ActionCode.PhotoToReplacement, fileName.Length);
                byte[] convertedfileName = Encoding.UTF8.GetBytes(fileName);

                long fileSize = this.fileLogic.GetFileSize(path);
                byte[] convertedfileSize = BitConverter.GetBytes(fileSize);

                await BytesSender(headerData, tcpClient);          // Envia header con orden y largo del nombre del archivo
                await BytesSender(convertedfileName, tcpClient);   // Envia nombre del archivo
                await BytesSender(convertedfileSize, tcpClient);   // Envia tamaño del archivo
                await FileSenderAsync(fileSize, path, tcpClient); // Envia archivo
            }
            else
            {
                throw new Exception("El archivo no existe");
            }
        }

        private byte[] HeaderCreator(ActionCode code, int dataLength)
        {
            int largoMensajeFijo = Constants.Msglength;
            int dataLengthBytes = dataLength.ToString().Length;
            int zerosToAdd = largoMensajeFijo - dataLengthBytes;
            string lengthOfMessage = "";
            int codeToInt = (int)code;
            int codeLength = codeToInt.ToString().Length;
            int zerosToAddToCode = Constants.CmdLength - codeLength;
            string lengthOfCode = "";

            for (int i = 0; i < zerosToAdd; i++)
            {
                lengthOfMessage += "0";
            }

            for (int i = 0; i < zerosToAddToCode; i++)
            {
                lengthOfCode += "0";
            }

            lengthOfMessage += dataLength.ToString();
            lengthOfCode += codeToInt.ToString();
            string header = lengthOfCode + lengthOfMessage;

            return Encoding.UTF8.GetBytes(header);
        }

        private byte[] FileHeaderCreator(int dataLength)
        {
            int largoMensajeFijo = Constants.Msglength;
            int dataLengthBytes = dataLength.ToString().Length;
            int zerosToAdd = largoMensajeFijo - dataLengthBytes;
            string lengthOfMessage = "";
            for (int i = 0; i < zerosToAdd; i++)
            {
                lengthOfMessage += "0";
            }

            lengthOfMessage += dataLength.ToString();
            string header = lengthOfMessage;

            return Encoding.UTF8.GetBytes(header);
        }

        private async Task FileSenderAsync(long fileSize, string path, TcpClient tcpClient)
        {
            long fileParts = Constants.FileParts(fileSize);
            long offset = 0;
            long currentPart = 1;
            int maxPacketSize = Constants.MaxPacketSize;
            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == fileParts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    data = fileHandler.Read(path, offset, lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    data = fileHandler.Read(path, offset, maxPacketSize);
                    offset += maxPacketSize;
                }

                await BytesSender(data, tcpClient);

                currentPart++;
            }
        }
    }
}
