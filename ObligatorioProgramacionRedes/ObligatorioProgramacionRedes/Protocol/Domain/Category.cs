using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Domain
{
    public class Category
    {
        public string categoryName { get; set; }
        public IList<Replacement> replacements { get; set; }
        public Category()
        {
            replacements = new List<Replacement>();
        }
    }
}
