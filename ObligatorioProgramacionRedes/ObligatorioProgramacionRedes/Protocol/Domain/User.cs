using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Domain
{
    public class User
    {
        public User()
        {
            messages = new List<string>();
        }
        public string? name { get; set; }
        public string? code { get; set; }
        public bool isAdmin { get; set; }
        public IList<string> messages { get; set; }
    }
}
