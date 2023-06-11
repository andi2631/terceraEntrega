using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Domain
{
    public class Replacement
    {
        public Replacement() {
            photo = "No tiene foto";
            categories = new List<Category>();
        }
        public string id { get; set; }
        public string name { get; set; }
        public string supplier { get; set; }
        public string brand { get; set; }
        public string photo { get; set; }
        public IList<Category> categories { get; set; }
    }
}
