namespace Protocol
{
    public class Constants
    {
        public const int LargoFijo = 6;
        public const int CmdLength = 2;
        public const int Msglength = 4;
        public const string Admin = "si";
        public const string NoAdmin = "no";
        public const string LoginOK = "Conectado";
        public const string LoginFalse = "Error";
        public const int idReplacement = 1;

        public const int FixedFileSize = 8;
        public static readonly int MaxPacketSize = 32768;

        public static long FileParts(long size)
        {
            var fileParts = size / MaxPacketSize;
            return fileParts * MaxPacketSize == size ? fileParts : fileParts + 1;
        }

    }
}