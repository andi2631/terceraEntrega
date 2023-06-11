using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class SettingsManager
    {
        public static string ReadSettings(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                return appSettings[key] ?? string.Empty; //Devuelve el valor de la key y si no encuentra devuelve lo que está a la derecha
                                                         //de los ??. En este caso string.Empty. Funciona como un if.
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error leyendo la configuracion");
                return string.Empty;
            }
        }
    }
}
