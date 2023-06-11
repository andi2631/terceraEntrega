using Microsoft.VisualBasic;
using Protocol.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protocol;

namespace Protocol.BuisnessLogic
{
    public class ReplacementLogic
    {
        public List<Replacement> replacements;
        int idProducto = Constants.idReplacement;

        public ReplacementLogic()
        {
            replacements = new List<Replacement>();
        }
        public bool AddReplacement(string replacement)
        {
            Replacement replacement1 = new Replacement();
            string[] newReplacement = replacement.Split("-");
            replacement1.id = "" + idProducto;
            replacement1.name = newReplacement[0];
            replacement1.supplier = newReplacement[1];
            replacement1.brand = newReplacement[2];
            if (!ReplacementExist(replacement1))
            {
                replacements.Add(replacement1);
                idProducto++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ReplacementExist(Replacement newReplacement)
        {
            foreach (Replacement r in replacements)
            {
                if (r.name.Equals(newReplacement.name))
                {
                    return true;
                }
            }
            return false;
        }
        public List<Replacement> AllReplacement()
        {
            return this.replacements;
        }

        public string GetAllReplacementsToString()
        {
            return ListToChain(AllReplacement());
        }

        private static string ListToChain(List<Replacement> lista)
        {
            string listaTostring = "";
            for (int i = 0; i < lista.Count(); i++)
            {
                string categories = ReplacementCategories(lista[i].categories);
                listaTostring += "\n" + "Nombre de repuesto: " + lista[i].name + ", Supplier: "+ lista[i].supplier + ", Marca: " + lista[i].brand + "\n" + "Categorias: " + categories + ", Foto: " +  lista[i].photo + "\n ";
            }
            return listaTostring;
        }

        public Replacement ObtainReplacement(object name)
        {
            return replacements.Where(item => item.name.Equals(name)).FirstOrDefault();
        }

        public string FilterByName(string nombre)
        {
            List<Replacement> filteredList = replacements.Where(item => item.name.Contains(nombre)).ToList();
            return ListToChain(filteredList);
        }

        public bool AddPhoto(Replacement rep, string name)
        {
            rep.photo = name;
            if (rep.photo.Length > 0)
            {
                return true;
            }
            return false;
        }

        public bool ExistReplacementByName(string newReplacement)
        {
            foreach (Replacement r in replacements)
            {
                if (r.name.Equals(newReplacement))
                {
                    return true;
                }
            }
            return false;
        }
        public void UpdateReplacement(Replacement r)
        {
            this.replacements.Remove(r);
            this.replacements.Add(r);
        }

        public static string ReplacementCategories(IList<Category> categories) {
            string result = "";
            if(categories.Count > 0)
            {
                foreach(Category c in categories)
                {
                    result += c.categoryName;
                }
            }
            else
            {
                result = "No tiene categorías asignadas";
            }
            return result;

        }

        public void AddRTestReplacement(Replacement r1)
        {
            this.replacements.Add(r1);
        }
    }
}
