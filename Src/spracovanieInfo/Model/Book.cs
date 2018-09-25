using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace spracovanieInfo.Model
{
    public class Book
    {
        public string BookName;
        [XmlAttribute("lang")]
        public string Language;

        public Book(string name, string language)
        {
            this.BookName = name;
            this.Language = language;
        }

        public Book()
        {

        }
    }
}
