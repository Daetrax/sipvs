using spracovanieInfo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace spracovanieInfo
{
    [XmlRoot(Namespace = "https://github.com/Daetrax/sipvs/blob/master/Xml_data/sipvt")]
    public class Request
    {
        public string Name;
        public string Surname;
        public string Street;
        public int StreetNumber;
        public string Country;
        public string City;
        public string Zip;
        public string RequestDate;
        public int LoanPeriod;

        public Book[] BookList;

        
        public Request(MainWindow form, List<Book> bookList)
        {
            // extract info from main window            
            this.Name = form.NameBox.Text;
            this.Surname = form.SurnameBox.Text;
            this.Street = form.StreetBox.Text;
            this.StreetNumber = Int32.Parse(form.StreetNumberBox.Text);
            this.Country = form.CountryBox.Text;
            this.City = form.CityBox.Text;
            this.Zip = form.ZipcodeBox.Text;
            this.LoanPeriod = Int32.Parse(form.LoanPeriodBox.Text);
            this.BookList = bookList.ToArray();
            // neccessary for timestamp
            this.RequestDate = DateTime.Now.ToString("yyyy-MM-dd");            
        }

        public Request()
        {

        }

    }
}
