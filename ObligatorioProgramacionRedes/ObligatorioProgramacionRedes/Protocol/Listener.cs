using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class Listener
    {
        public FileHandler fileHandler;
        private readonly ConversionHandler conversionHandler;

        public Listener()
        {
            fileHandler = new FileHandler();
            conversionHandler = new ConversionHandler();
        }

        public async Task<Dictionary<string, string>> ReceiveData(TcpClient tcpClient)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            int headerTotalLength = Constants.LargoFijo;

            byte[] header = await RealReceiverAsync(headerTotalLength, tcpClient);

            string headerToString = Encoding.UTF8.GetString(header);
            string order = headerToString.Substring(0, Constants.CmdLength);
            if ((ActionCode)Int32.Parse(order) != ActionCode.PhotoToReplacement)
            {
                int messagelength = Int32.Parse(headerToString.Substring(Constants.CmdLength));
                byte[] data = await RealReceiverAsync(messagelength, tcpClient);

                ret.Add("Codigo", order);
                ret.Add("Mensaje", Encoding.UTF8.GetString(data));
            }
            else
            {
                ret.Add("Codigo", order);
                int fileNameSize = Int32.Parse(headerToString.Substring(Constants.CmdLength));
                await ReceiveFileAsync(fileNameSize, tcpClient);
            }
            return ret;
        }


        private async Task<byte[]> RealReceiverAsync(int length, TcpClient tcpClient)
        {
            byte[] response = new byte[length];
            int offset = 0;
            NetworkStream networkStream = tcpClient.GetStream();
            while (offset < length)
            {
                int received = await networkStream.ReadAsync(response, offset, length - offset);

                if (received == 0)
                {
                    throw new SocketException();
                }
                offset += received;
            }

            return response;
        }

        public async Task ReceiveFileAsync(int fileNameSize, TcpClient tcpClient)
        {
            string fileName =  Encoding.UTF8.GetString(await RealReceiverAsync(fileNameSize, tcpClient));
            long fileSize = BitConverter.ToInt64(await RealReceiverAsync(Constants.FixedFileSize, tcpClient));

            await FileStreamReceiverAsync(fileSize, fileName, tcpClient);
        }

        private async Task FileStreamReceiverAsync(long fileSize, string fileName, TcpClient tcpClient)
        {
            long fileParts = Constants.FileParts(fileSize);
            long offset = 0;
            long currentPart = 1;
            byte[] data;

            while (fileSize > offset)
            {
                if (currentPart == fileParts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    data = await RealReceiverAsync(lastPartSize, tcpClient);
                    offset += lastPartSize;
                }
                else
                {
                    data = await RealReceiverAsync(Constants.MaxPacketSize, tcpClient);
                    offset += Constants.MaxPacketSize;
                }

                fileHandler.Write(fileName, data);
                currentPart++;
            }

        }

    }
    public enum ActionCode
    {
        CreateUser,
        CreateReplacement,
        CreateCategories,
        AssignCategories,
        AssignPhoto,
        GetReplacements,
        GetSpecificReplacement,
        SendMessage,
        ReadMessages,
        Login,
        PhotoToReplacement,
    }
}
