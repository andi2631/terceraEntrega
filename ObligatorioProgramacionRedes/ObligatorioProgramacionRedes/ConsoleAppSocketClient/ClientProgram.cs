using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Protocol;

namespace ConsoleAppSocketClient
{
    public class ClientProgram
    {
        public class Servidor
        {
            static Sender sender = new Sender();
            static Listener listener = new Listener();
            static bool isAdminLoggedUser;
            static TcpClient tcpClient;
            static string loggedUserName;

            static async Task Main(string[] args)
            {
                Sender sender = new Sender();
                Listener listener = new Listener();

                Console.WriteLine("Iniciando Cliente");
                Console.WriteLine("\n");
                string clientIp = SettingsManager.ReadSettings(ClientConfig.clientIpAddress);
                string serverIp = SettingsManager.ReadSettings(ClientConfig.serverIpAddress);
                int serverPort = int.Parse(SettingsManager.ReadSettings(ClientConfig.serverPortconfigKey));
                var endpointCliente = new IPEndPoint(IPAddress.Parse(clientIp), 0);
                tcpClient = new TcpClient(endpointCliente);
                var endpointServidor = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
                await tcpClient.ConnectAsync(endpointServidor);
                try
                {
                    await LoginAsync(tcpClient);
                    await using (var netStream = tcpClient.GetStream())
                    {
                        bool menu = true;
                        while (menu)
                        {
                            menu = await ShowMenu(tcpClient);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("No se ha podido conectar con el servidor, reinicie la aplicación e intente nuevamente");
                }


            }

            private static async Task LoginAsync(TcpClient tcpClient)
            {
                bool notlogged = true;
                while (notlogged)
                {
                    notlogged = await LoginHandlerAsync(tcpClient);
                    if (notlogged) {
                        Console.WriteLine("Su usuario/contraseña son invalidos");
                    }
                }
            }

            private static async Task<bool> LoginHandlerAsync(TcpClient tcpClient)
            {
                string loginData = "";
                Console.Write("----BIENVENIDO AL SISTEMA----");
                Console.WriteLine("\n");
                Console.Write("\nIngrese usuario: ");
                string nombreUsuario = Console.ReadLine();
                loginData += nombreUsuario + "-";
                Console.Write("Ingrese contraseña: ");
                string contraseniaUsuarui = Console.ReadLine();
                loginData += contraseniaUsuarui;
                await sender.Send(ActionCode.Login, loginData, tcpClient);
                Dictionary<string,string> answer = await listener.ReceiveData(tcpClient);
                string[] respuesta = answer["Mensaje"].Split("-");
                if (respuesta[0].ToUpper() == Constants.LoginOK.ToUpper())
                {
                    isAdminLoggedUser = bool.Parse(respuesta[1]);
                    loggedUserName = nombreUsuario;
                    return false;
                }
                else return true;
            }

            private static async Task<bool> ShowMenu(TcpClient tcpClient)
            {

                Console.WriteLine("\n                         MENU PRINCIPAL: ");
                Console.WriteLine("\n                         (1)   CREAR USUARIO");
                Console.WriteLine("\n                         (2)   CREAR REPUESTO");
                Console.WriteLine("\n                         (3)   CREAR CATEGORIA");
                Console.WriteLine("\n                         (4)   ASOCIAR CATEGORIA A REPUESTO");
                Console.WriteLine("\n                         (5)   ASOCIAR FOTO A REPUESTO");
                Console.WriteLine("\n                         (6)   CONSULTAR TODOS LOS REPUESTOS");
                Console.WriteLine("\n                         (7)   CONSULTAR REPUESTO");
                Console.WriteLine("\n                         (8)   ENVIAR MENSAJE");
                Console.WriteLine("\n                         (9)   BANDEJA DE ENTRADA");
                Console.WriteLine("\n                         (0)   SALIR\n");
                Console.WriteLine("Seleccionar una opción valida: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        if (isAdminLoggedUser)
                        {
                            await CreateUserAsync();
                        }
                        else { Console.WriteLine("EL USUARIO NO ES ADMINISTRADOR!"); }
                        return true;
                    case "2":
                        await CreateReplacementAsync();
                        return true;
                    case "3":
                        await CreateCategoryAsync();
                        return true;
                    case "4":
                        await AssignCategoryAsync();
                        return true;
                    case "5":
                        await AssignPhotoAsync();
                        return true;
                    case "6":
                        await GetAllReplacementsAsync();
                        return true;
                    case "7":
                        await GetReplacementByNameAsync();
                        return true;
                    case "8":
                        await SendMessageAsync();
                        return true;
                    case "9":
                        await MessageMenuAsync();
                        return true;
                    case "0":
                        tcpClient.Close();
                        return false;
                    default:
                        Console.WriteLine("Verifique el número ingresado...");
                        return true;
                }

            }

            private static async Task AssignPhotoAsync()
            {
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("|INDICAR NOMBRE DEL REPUESTO|");
                string fullMessage = Console.ReadLine();
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("|ASIGNAR UNA FOTO AL REPUESTO|");
                Console.WriteLine("---------------------------------------------");
                string path = Console.ReadLine();
                await sender.SendFile(path, tcpClient);
                fullMessage += "*" + path;
                await sender.Send(ActionCode.AssignPhoto, fullMessage, tcpClient);
                Dictionary<string, string> answer = await listener.ReceiveData(tcpClient);
                Console.WriteLine(answer["Mensaje"]);
                /*Esta funcionalidad todavía sigue en desarrollo. Al enviar el archivo, el programa se tranca*/
            }

            private static async Task GetReplacementByNameAsync()
            {
                string fullMessage = "";

                Console.WriteLine("INGRESE UN NOMBRE PARA FILTRAR EL REPUESTO: ");

                fullMessage += Console.ReadLine();

                await sender.Send(ActionCode.GetSpecificReplacement, fullMessage, tcpClient);
                Console.WriteLine("Los Repuestos obtenidos fueron los siguientes: ");
                Dictionary<string, string> answer = await listener.ReceiveData(tcpClient);
                Console.WriteLine(answer["Mensaje"]);

            }


            private static async Task GetAllReplacementsAsync()
            {
                await sender.Send(ActionCode.GetReplacements, "", tcpClient);
                Console.WriteLine("Los Repuestos obtenidos fueron los siguientes: ");
                Dictionary<string, string> answer = await listener.ReceiveData(tcpClient);
                Console.WriteLine(answer["Mensaje"]);
            }

            private static async Task SendMessageAsync()
            {
                string fullMessage = "";

                Console.WriteLine("----------------------");
                Console.WriteLine("CENTRO DE MENSAJERIA");
                Console.WriteLine("----------------------");
                Console.WriteLine("");
                Console.WriteLine("ESCRIBA A QUIEN QUIERE ENVIARLE EL MENSAJE: ");

                fullMessage += Console.ReadLine() + "-";
                Console.WriteLine("ESCRIBA EL MENSAJE QUE DESEA ENVIAR: ");

                fullMessage += loggedUserName + "-" +  Console.ReadLine();

                await sender.Send(ActionCode.SendMessage, fullMessage, tcpClient);
                Dictionary<string, string> answer = await listener.ReceiveData(tcpClient);
                Console.WriteLine(answer["Mensaje"]);

            }

            private static async Task AssignCategoryAsync()
            {
                string fullMessage = "";

                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("|ASIGNAR CATEGORÍA AL REPUESTO|");
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("Ingrese nombre de categoría: ");
                fullMessage += Console.ReadLine() + "-";
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("Ingrese nombre del repuesto: ");
                fullMessage += Console.ReadLine();
                await sender.Send(ActionCode.AssignCategories, fullMessage, tcpClient);
                Dictionary<string, string> answer = await listener.ReceiveData(tcpClient);
                Console.WriteLine(answer["Mensaje"]);

            }

            private static async Task CreateCategoryAsync()
            {
                string fullMessage = "";

                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("|CREAR CATEGORÍA|");
                Console.WriteLine("---------------------------------------------");

                Console.WriteLine("Ingrese nombre de categoría: ");

                fullMessage += Console.ReadLine();
                await sender.Send(ActionCode.CreateCategories, fullMessage, tcpClient);
                Dictionary<string, string> answer = await listener.ReceiveData(tcpClient);
                Console.WriteLine(answer["Mensaje"]);
            }

            private static async Task CreateReplacementAsync()
            {
                string fullMessage = "";

                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("|BIENVENIDO AL MENU PARA CREAR UN REPUESTO|");
                Console.WriteLine("---------------------------------------------");

                Console.WriteLine("\n Ingrese el nombre del repuesto: ");

                fullMessage += Console.ReadLine() + "-";

                Console.WriteLine("Ingrese el nombre del proveedor del repuesto: ");

                fullMessage += Console.ReadLine() + "-";

                Console.WriteLine("Ingrese la marca del repuesto: ");

                fullMessage += Console.ReadLine();

                await sender.Send(ActionCode.CreateReplacement, fullMessage, tcpClient);
                Dictionary<string, string> answer = await listener.ReceiveData(tcpClient);
                Console.WriteLine(answer["Mensaje"]);

            }

            private static async Task MessageMenuAsync()
            {
                await sender.Send(ActionCode.ReadMessages, loggedUserName, tcpClient);
                Dictionary<string, string> answer = await listener.ReceiveData(tcpClient);
                Console.WriteLine(answer["Mensaje"]);
            }

            private static async Task CreateUserAsync()
            {
                string fullMessage = "";

                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("|BIENVENIDO AL MENU PARA CREAR UN USUARIO|");
                Console.WriteLine("-----------------------------------------");

                Console.WriteLine("INGRESE UN NOMBRE");

                fullMessage += Console.ReadLine() + "-";

                Console.WriteLine("INGRESE UNA CONTRASEÑA: ");

                fullMessage += Console.ReadLine();

                await sender.Send(ActionCode.CreateUser, fullMessage, tcpClient);
                Dictionary<string, string> answer = await listener.ReceiveData(tcpClient);
                Console.WriteLine(answer["Mensaje"]);
            }
        }
    }
}
