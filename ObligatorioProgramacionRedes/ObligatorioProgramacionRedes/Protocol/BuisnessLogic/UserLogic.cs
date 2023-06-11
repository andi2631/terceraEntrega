using Protocol.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Protocol.BuisnessLogic
{
    public class UserLogic
    {
        private IList<User> users;
        public UserLogic()
        {
            users = new List<User>();
        }

        public bool UserExist(User user)
        {
            foreach (User u in users)
            {
                if (u.name.Equals(user.name) && u.code.Equals(user.code))
                {
                    return true;
                }
            }
            return false;
        }

        public void AddAdminUser(User admin)
        {
            this.users.Add(admin);
        }

        public void AddTestUser(User userTest)
        {
            this.users.Add(userTest);
        }
        public bool AddUserAdmin(string user)
        {
            User newUser = transformUser(user);
            newUser.isAdmin = true;
            if (!UserNameExist(newUser))
            {
                users.Add(newUser);
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool AddUser(string user)
        {
            User newUser = transformUser(user);
            newUser.isAdmin = false;
            if (!UserNameExist(newUser))
            {
                users.Add(newUser);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool UserNameExist(User newUser)
        {
            foreach (User u in users)
            {
                if (u.name.Equals(newUser.name))
                {
                    return true;
                }
            }
            return false;
        }

        private User transformUser(string user)
        {
            string[] myUser = user.Split("-");
            User newUser = new User();
            newUser.name = myUser[0];
            newUser.code = myUser[1];
            return newUser;
        }

        public User LoginUser(string user)
        {
            string[] myUser = user.Split("-");
            User newUser = transformUser(user);
            if (UserExist(newUser))
            {
                return users.Where<User>(u => u.name.Equals(newUser.name) && u.code.Equals(u.code)).FirstOrDefault();
            }
            return null;
        }

        private bool TransformUserAdmin(string isAdmin)
        {
            if (isAdmin.ToUpper().Equals("SI"))
            {
                return true;
            }
            return false;
        }


        private bool IsUserAdmin(bool isAdmin)
        {
            if (isAdmin)
            {
                return true;
            }
            return false;
        }

        public bool SendMessage(string message)
        {
            string[] splittedMessage = message.Split("-");

            string DestinataryName = splittedMessage[0];
            string mainMessage = splittedMessage[2];
            string sender = splittedMessage[1];

            if (users.Where<User>(u => u.name == DestinataryName).FirstOrDefault() != null)
            {
                users.Where<User>(u => u.name == DestinataryName).FirstOrDefault().messages.Add($"\n     Enviado por: {sender} " + ", Mensaje: " + mainMessage);
                return true;
            }
            else return false;
        }

        public string ReadMessages(string name)
        {
            string messageTuReturn = "";

            User reader = users.Where<User>(u => u.name.ToLower() == name.ToLower()).FirstOrDefault();
            if (reader.messages.Count() == 0) return "NO TIENE MENSAJES PARA LEER!";
            else
            {
                for (int i = 0; i < reader.messages.Count(); i++)
                {
                    messageTuReturn += reader.messages[i] + " \n";
                }

                return messageTuReturn;
            }
        }

        public void ViewedMenssages(string userName)
        {
            User reader = users.Where<User>(u => u.name.ToLower() == userName.ToLower()).FirstOrDefault();
            for(int i=0; i<reader.messages.Count();i++)
            {
                if (!reader.messages[i].Contains("(Leído)")) {
                    string message = reader.messages[i];
                    message += "(Leído)";
                    reader.messages.Remove(reader.messages[i]);
                    reader.messages.Add(message);
                }
            }
        }

        private void UpdateMessages(string userName) {
            
        
        }
    }
}
