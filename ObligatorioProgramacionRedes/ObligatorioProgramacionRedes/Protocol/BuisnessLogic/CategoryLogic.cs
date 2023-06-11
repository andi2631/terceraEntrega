using Protocol.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.BuisnessLogic
{
    public class CategoryLogic
    {
        private IList<Category> categories;
        private ReplacementLogic replacementLogic;

        public CategoryLogic(ReplacementLogic replacementLogic)
        {
            categories = new List<Category>();
            this.replacementLogic = replacementLogic;
        }
        public bool AddCategory(string category)
        {
            Category newCategory = new Category();
            newCategory.categoryName = category;
            if (!CategoryExist(newCategory.categoryName))
            {
                categories.Add(newCategory);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CategoryExist(string newCategory)
        {
            foreach (Category cat in categories)
            {
                if (cat.categoryName.Equals(newCategory))
                {
                    return true;
                }
            }
            return false;
        }

        private bool ReplacementExistInCategory(Category c, string replacementName)
        {
            foreach (Replacement r in c.replacements)
            {
                if (r.name.Equals(replacementName))
                {
                    return true;
                }

            }
            return false;
        }

        private Category ObtainCategory(string newCategory)
        {
            foreach (Category c in categories)
            {
                if (c.categoryName.Equals(newCategory))
                {
                    return c;
                }
            }
            return null;
        }

        public bool AddReplacementToCategory(string message)
        {
            string[] categoryRep = message.Split("-");
            if (CategoryExist(categoryRep[0]))
            {
                Category c = ObtainCategory(categoryRep[0]);
                Replacement r = ObtainReplacement(categoryRep[1]);
                if (replacementLogic.ReplacementExist(r) && !ReplacementExistInCategory(c, categoryRep[1]))
                {
                    c.replacements.Add(r);
                    r.categories.Add(c);
                    UpdateCategory(c);
                    UpdateReplacement(r);
                    return true;
                }
            }
            return false;
        }

        private void UpdateReplacement(Replacement r)
        {
            this.replacementLogic.UpdateReplacement(r);
        }

        private void UpdateCategory(Category category)
        {
            this.categories.Remove(category);
            this.categories.Add(category);
        }

        public Replacement ObtainReplacement(string replacementName)
        {
            return replacementLogic.ObtainReplacement(replacementName);
        }

        public void AddTestCategory(Category category)
        {
            this.categories.Add(category);
        }
    }
}
