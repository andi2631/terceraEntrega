using Protocol.BuisnessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppSocketServer
{
    public class ServerStart
    {
        public static async Task Main(string[] args)
        {
            UserLogic userLogic = new UserLogic();
            ReplacementLogic replacementLogic = new ReplacementLogic();
            CategoryLogic categoryLogic = new CategoryLogic(replacementLogic);
            Program serverProgram = new Program(userLogic, replacementLogic, categoryLogic);

            await serverProgram.ListenerAsync();
        }
    }
}