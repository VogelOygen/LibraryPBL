using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryPBL.Model
{
    #region списки

    enum Category
    {
        Programming,
        Design,
        Web,
        AI
    }
    enum Language
    {
        Russian,
        English,
        Uzbek
    }
    
    #endregion

    internal class Book
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Publish { get; set; }
        public List<Category> Category { get; set; }
        public List<Language> Language { get; set; }

        public string allInfo()
        {
            var languages = string.Join(", ", Language);
            var categories = string.Join(", ", Category);

            return $"--> {Name} | {Author} | ISBN: {ISBN} | PUBLISCH: {Publish} | Lang: {languages}  | Category: {categories} |";
        }

        public string smallInfo()
        {
            var languages = string.Join(", ", Language);
            var categories = string.Join(", ", Category);

            return $"--> {Name} | {Author} | Category: {categories} |";
        }
    }
}
