using Protocol.BuisnessLogic;
using Protocol.Domain;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Xml.Linq;

namespace ConsoleAppSocketServer
{
    public class Program
    {
        private TcpListener tcpListener;
        private UserLogic UserLogic;
        private ReplacementLogic replacementLogic;
        private CategoryLogic CategoryLogic;
        private Listener listener;
        private Sender sender;
        private int connectedClients = 0;

        public Program(UserLogic userLogic, ReplacementLogic replacementLogic, CategoryLogic categoryLogic)
        {
            this.UserLogic = userLogic;
            this.replacementLogic = replacementLogic;
            this.CategoryLogic = categoryLogic;
            this.listener = new Listener();
            CreateAdminUser();
            DatosPrueba();
            this.sender = new Sender();
            string serverIp = SettingsManager.ReadSettings(ServerConfig.serverIPconfigKey);
            int serverPort = int.Parse(SettingsManager.ReadSettings(ServerConfig.serverPortconfigKey));
            var endPointServidor = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
            this.tcpListener = new TcpListener(endPointServidor);
            Console.WriteLine($"Server iniciado con IP {serverIp} y Puerto {serverPort}...");

        }

        private void DatosPrueba() {
            Replacement r1 = new Replacement()
            {
                name = "Ruleman",
                supplier = "Juani",
                brand = "MyD",
            };
            User client = new User()
            {
                name = "Juani",
                code = "Juani1234",
                isAdmin = false,
            };

            Category category = new Category()
            {
                categoryName = "Rulemanes"

            };

            this.replacementLogic.AddRTestReplacement(r1);
            this.UserLogic.AddTestUser(client);
            this.CategoryLogic.AddTestCategory(category);


        }

        private void CreateAdminUser() //Hardcodeo del usuario admin
        {
            User admin = new User()
            {
                name = SettingsManager.ReadSettings(ServerConfig.NombreAdmin),
                code = SettingsManager.ReadSettings(ServerConfig.CodeAdmin),
                isAdmin = true
            };
            this.UserLogic.AddAdminUser(admin);
            Console.WriteLine(admin.name + " " + admin.code);
        }

        public async Task ListenerAsync()
        {
            int maxClients = int.Parse(SettingsManager.ReadSettings(ServerConfig.MaxClients));
            this.tcpListener.Start(maxClients);

            while (connectedClients < maxClients)
            {
                // 5- Aceptamos N conexiones
                var tcpClientSocket = await tcpListener.AcceptTcpClientAsync(); //Bloquea el hilo principal del Servidor, queda esperando por una conexión de un Cliente
                Console.WriteLine("Cliente conectado!");
                connectedClients++;
                Console.WriteLine(connectedClients);
                // Disparamos un hilo para manejar cada Cliente
                var task = Task.Run(async () => await ManejoCliente(tcpClientSocket));
            }
            Console.ReadLine();
            Console.WriteLine("Se termino ejecución");
        }

        public async Task ManejoCliente(TcpClient tcpClient)
        {
            bool serverStatus = true;

            while (serverStatus)
            {
                try
                {
                    Dictionary<string, string> data = await this.listener.ReceiveData(tcpClient);
                    ActionCode code = (ActionCode)Int32.Parse(data["Codigo"]);

                    if (code != ActionCode.PhotoToReplacement)
                    {
                        string message = data["Mensaje"];

                        Console.WriteLine(code + "----" + data);
                        await ActionHandler(code, message, tcpClient);
                    }
                    else {
                        Console.WriteLine("Foto ha llegado con exito");

                    }


                }
                catch (SocketException e)
                {
                    connectedClients--;
                    serverStatus = false;
                }

            }
            Console.WriteLine("Cliente desconectado");
            Console.WriteLine(connectedClients);
        }

        public async Task ActionHandler(ActionCode code, string message, TcpClient tcpClient)
        {
            try
            {
                switch (code)
                {
                    case ActionCode.CreateUser:
                        bool newUser = this.UserLogic.AddUser(message);
                        if (newUser) await ResponseSender(ActionCode.CreateUser, tcpClient, "USUARIO CREADO CORRECTAMENTE");
                        else await ResponseSender(ActionCode.CreateUser, tcpClient, "EL USUARIO YA EXISTE");
                        break;
                    case ActionCode.CreateReplacement:
                        bool result = this.replacementLogic.AddReplacement(message);
                        if (result) await ResponseSender(ActionCode.CreateReplacement, tcpClient, "REPUESTO AGREGADO CORRECTAMENTE");
                        else await ResponseSender(ActionCode.CreateReplacement, tcpClient, "EL REPUESTO YA EXISTE");
                        break;
                    case ActionCode.AssignCategories:
                        bool assignCat = this.CategoryLogic.AddReplacementToCategory(message);
                        if (assignCat) await ResponseSender(ActionCode.CreateReplacement, tcpClient, "SE ASIGNO EL REPUESTO A UNA CATEGORIA CON EXITO");
                        else await ResponseSender(ActionCode.CreateReplacement, tcpClient, "VERIFIQUE QUE EL NOMBRE DEL REPUESTO/CATEGORIA ESTE ESCRITO CORRECTAMENTE O QUE EL REPUESTO YA EXISTA EN ESA CATEGORÍA");
                        break;
                    case ActionCode.CreateCategories:
                        bool createCategory = this.CategoryLogic.AddCategory(message);
                        if (createCategory) await ResponseSender(ActionCode.CreateCategories, tcpClient, "CATEGORIA CREADA CORRECTAMENTE");
                        else await ResponseSender(ActionCode.CreateReplacement, tcpClient, "CATEGORIA YA EXISTE");
                        break;
                    case ActionCode.GetReplacements:
                        if(this.replacementLogic.GetAllReplacementsToString().Length >0) await ResponseSender(ActionCode.GetReplacements, tcpClient, this.replacementLogic.GetAllReplacementsToString());
                        else await ResponseSender(ActionCode.GetReplacements, tcpClient, "NO HAY REPUESTOS EXISTENTES");
                        break;
                    case ActionCode.Login:
                        User user = this.UserLogic.LoginUser(message);
                        if (user != null) await ResponseSender(ActionCode.Login, tcpClient, Constants.LoginOK + "-" + user.isAdmin.ToString());
                        else await ResponseSender(ActionCode.Login, tcpClient, Constants.LoginFalse + "-" + "NOT LOGGED");
                        break;
                    case ActionCode.GetSpecificReplacement:
                        if (this.replacementLogic.FilterByName(message) != null) await ResponseSender(ActionCode.GetReplacements, tcpClient, this.replacementLogic.FilterByName(message));
                        else await ResponseSender(ActionCode.GetSpecificReplacement, tcpClient, "NO EXISTEN REPUESTOS CON ESE NOMBRE");
                        break;
                    case ActionCode.SendMessage:
                        bool sent = this.UserLogic.SendMessage(message);
                        if (sent) await ResponseSender(ActionCode.SendMessage, tcpClient, "MENSAJE ENVIADO CON EXITO!");
                        else await ResponseSender(ActionCode.SendMessage, tcpClient, "EL USUARIO SELECCIONADO PARA ENVIAR UN MENSAJE NO EXISTE");
                        break;
                    case ActionCode.ReadMessages:
                        await ResponseSender(ActionCode.ReadMessages, tcpClient, this.UserLogic.ReadMessages(message));
                        this.UserLogic.ViewedMenssages(message);
                        break;
                    case ActionCode.AssignPhoto:
                        string[] info = message.Split("*");
                        Replacement rep = this.replacementLogic.ObtainReplacement(info[0]);
                        if (rep.Equals(null))
                        {
                         await ResponseSender(ActionCode.AssignPhoto, tcpClient, "NO EXISTE REPUESTO");
                        }
                        else
                        {
                            bool photo = this.replacementLogic.AddPhoto(rep, info[1]); 
                            if (photo)
                            {
                                await ResponseSender(ActionCode.AssignPhoto, tcpClient, "FOTO AGREGADA CON EXITO");
                            }
                            else
                            {
                                await ResponseSender(ActionCode.AssignPhoto, tcpClient, "NO EXISTE ARCHIVO ENVIADO");
                            }
                        }
                        break;
                        

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private async Task ResponseSender(ActionCode code, TcpClient tcpClient, string message)
        {
            await sender.Send(code, message, tcpClient);
        }
    }
}
